using System.Data;
using MySqlConnector;

namespace HolidayMaker.Repositories;

public sealed record CustomerOption(int Id, string Name);

public enum BookingStatus
{
    Created,
    InvalidInput,
    CustomerNotFound,
    RoomNotFound,
    CapacityExceeded,
    RoomNotAvailable
}

public sealed record BookingResult(
    BookingStatus Status,
    long BookingId = 0,
    decimal TotalPrice = 0);

public class BookingRepository
{
    private readonly string _connectionString;

    public BookingRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<CustomerOption>> GetCustomersAsync()
    {
        var customers = new List<CustomerOption>();

        const string sql = """
            SELECT id, name
            FROM customer
            ORDER BY id;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            customers.Add(new CustomerOption(
                reader.GetInt32(0),
                reader.GetString(1)));
        }

        return customers;
    }

    public async Task<BookingResult> CreateBookingAsync(
        int customerId,
        int roomId,
        DateTime checkIn,
        DateTime checkOut,
        int guests)
    {
        if (customerId <= 0 || roomId <= 0 || guests <= 0 ||
            checkIn >= checkOut ||
            checkIn.Year != 2026 || checkIn.Month != 12 ||
            checkOut.Year != 2026 || checkOut.Month != 12)
        {
            return new BookingResult(BookingStatus.InvalidInput);
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Samtliga kontroller och bokningen sker inom samma transaktion.
        await using var transaction =
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        const string customerSql = """
            SELECT COUNT(*)
            FROM customer
            WHERE id = @customerId;
            """;

        await using (var command = new MySqlCommand(customerSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@customerId", customerId);
            var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
            if (!exists)
            {
                await transaction.RollbackAsync();
                return new BookingResult(BookingStatus.CustomerNotFound);
            }
        }

        // Lås rumsraden så två samtidiga bokningar av samma rum
        // inte kan passera tillgänglighetskontrollen samtidigt.
        const string roomSql = """
            SELECT max_guests, price_per_night
            FROM room
            WHERE id = @roomId
            FOR UPDATE;
            """;

        int maxGuests = 0;
        decimal pricePerNight = 0;
        bool roomFound;
        await using (var command = new MySqlCommand(roomSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@roomId", roomId);
            await using var reader = await command.ExecuteReaderAsync();
            roomFound = await reader.ReadAsync();
            if (roomFound)
            {
                maxGuests = reader.GetInt32(0);
                pricePerNight = Convert.ToDecimal(reader.GetValue(1));
            }
        }

        // Läsa färdigt och stäng DataReader innan transaktionen rullas tillbaka.
        if (!roomFound)
        {
            await transaction.RollbackAsync();
            return new BookingResult(BookingStatus.RoomNotFound);
        }

        if (guests > maxGuests)
        {
            await transaction.RollbackAsync();
            return new BookingResult(BookingStatus.CapacityExceeded);
        }

        const string availabilitySql = """
            SELECT COUNT(*)
            FROM booking
            WHERE room_id = @roomId
              AND start_date < @checkOut
              AND end_date > @checkIn;
            """;

        await using (var command = new MySqlCommand(availabilitySql, connection, transaction))
        {
            command.Parameters.AddWithValue("@roomId", roomId);
            command.Parameters.AddWithValue("@checkIn", checkIn);
            command.Parameters.AddWithValue("@checkOut", checkOut);

            var overlappingBookings =
                Convert.ToInt32(await command.ExecuteScalarAsync());

            if (overlappingBookings > 0)
            {
                await transaction.RollbackAsync();
                return new BookingResult(BookingStatus.RoomNotAvailable);
            }
        }

        const string insertSql = """
            INSERT INTO booking
                (customer_id, room_id, start_date, end_date, number_of_guests)
            VALUES
                (@customerId, @roomId, @checkIn, @checkOut, @guests);
            """;

        long bookingId;
        await using (var command = new MySqlCommand(insertSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@customerId", customerId);
            command.Parameters.AddWithValue("@roomId", roomId);
            command.Parameters.AddWithValue("@checkIn", checkIn);
            command.Parameters.AddWithValue("@checkOut", checkOut);
            command.Parameters.AddWithValue("@guests", guests);

            await command.ExecuteNonQueryAsync();
            bookingId = command.LastInsertedId;
        }

        await transaction.CommitAsync();

        decimal totalPrice = (checkOut - checkIn).Days * pricePerNight;
        return new BookingResult(BookingStatus.Created, bookingId, totalPrice);
    }
}
