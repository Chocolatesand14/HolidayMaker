
using System.Data;
using MySqlConnector;

namespace HolidayMaker.Repositories;

public record CustomerOption(int Id, string Name);

public enum BookingStatus
{
    Created,
    InvalidInput,
    CustomerNotFound,
    RoomNotFound,
    CapacityExceeded,
    RoomNotAvailable
}

public record BookingResult(
    BookingStatus Status,
    long BookingId = 0,
    decimal TotalPrice = 0
);

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

        await using var connection =
            new MySqlConnection(_connectionString);

        await connection.OpenAsync();

        const string sql = """
            SELECT id, name
            FROM customer
            ORDER BY name;
            """;

        await using var command =
            new MySqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            customers.Add(new CustomerOption(
                reader.GetInt32(0),
                reader.GetString(1)
            ));
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
        if (customerId <= 0 ||
            roomId <= 0 ||
            guests <= 0 ||
            checkIn.Year != 2026 ||
            checkIn.Month != 12 ||
            checkOut.Year != 2026 ||
            checkOut.Month != 12 ||
            checkOut <= checkIn)
        {
            return new BookingResult(BookingStatus.InvalidInput);
        }

        await using var connection =
            new MySqlConnection(_connectionString);

        await connection.OpenAsync();

        // Keep the checks and INSERT in one transaction.
        await using var transaction =
            await connection.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        // Check that the customer exists.
        const string customerSql = """
            SELECT COUNT(*)
            FROM customer
            WHERE id = @customerId;
            """;

        await using (var customerCommand =
            new MySqlCommand(customerSql, connection, transaction))
        {
            customerCommand.Parameters.AddWithValue(
                "@customerId", customerId);

            long count = Convert.ToInt64(
                await customerCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                return new BookingResult(
                    BookingStatus.CustomerNotFound);
            }
        }

        // Lock the room while checking and creating the booking.
        const string roomSql = """
            SELECT max_guests, price_per_night
            FROM room
            WHERE id = @roomId
            FOR UPDATE;
            """;

        int maxGuests;
        decimal pricePerNight;

        await using (var roomCommand =
            new MySqlCommand(roomSql, connection, transaction))
        {
            roomCommand.Parameters.AddWithValue("@roomId", roomId);

            await using var reader =
                await roomCommand.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return new BookingResult(
                    BookingStatus.RoomNotFound);
            }

            maxGuests = reader.GetInt32(0);
            pricePerNight = reader.GetDecimal(1);
        }

        if (guests > maxGuests)
        {
            return new BookingResult(
                BookingStatus.CapacityExceeded);
        }

        // A room is unavailable only when the periods overlap.
        const string availabilitySql = """
            SELECT COUNT(*)
            FROM booking
            WHERE room_id = @roomId
              AND start_date < @checkOut
              AND end_date > @checkIn;
            """;

        await using (var availabilityCommand =
            new MySqlCommand(
                availabilitySql, connection, transaction))
        {
            availabilityCommand.Parameters.AddWithValue(
                "@roomId", roomId);

            availabilityCommand.Parameters.AddWithValue(
                "@checkIn", checkIn);

            availabilityCommand.Parameters.AddWithValue(
                "@checkOut", checkOut);

            long overlappingBookings = Convert.ToInt64(
                await availabilityCommand.ExecuteScalarAsync());

            if (overlappingBookings > 0)
            {
                return new BookingResult(
                    BookingStatus.RoomNotAvailable);
            }
        }

        // Save the booking.
        const string insertSql = """
            INSERT INTO booking
                (customer_id, room_id, start_date,
                 end_date, number_of_guests)
            VALUES
                (@customerId, @roomId, @checkIn,
                 @checkOut, @guests);
            """;

        await using var insertCommand =
            new MySqlCommand(insertSql, connection, transaction);

        insertCommand.Parameters.AddWithValue(
            "@customerId", customerId);

        insertCommand.Parameters.AddWithValue(
            "@roomId", roomId);

        insertCommand.Parameters.AddWithValue(
            "@checkIn", checkIn);

        insertCommand.Parameters.AddWithValue(
            "@checkOut", checkOut);

        insertCommand.Parameters.AddWithValue(
            "@guests", guests);

        await insertCommand.ExecuteNonQueryAsync();

        long bookingId = insertCommand.LastInsertedId;

        int nights = (checkOut - checkIn).Days;
        decimal totalPrice = nights * pricePerNight;

        await transaction.CommitAsync();

        return new BookingResult(
            BookingStatus.Created,
            bookingId,
            totalPrice
        );
    }
}
