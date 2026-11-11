using System.Data;
using MySqlConnector;

namespace HolidayMaker.Repositories;

public enum BookingChangeStatus
{
    Updated,
    Cancelled,
    BookingNotFound,
    CapacityExceeded,
    InvalidInput
}

// Separat repository för ändringar av befintliga bokningar.
// Din befintliga BookingRepository (skapa bokning) behöver inte ändras.
public class BookingManagementRepository
{
    private readonly string _connectionString;

    public BookingManagementRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<BookingChangeStatus> UpdateGuestsAsync(
        int customerId, int bookingId, int guests)
    {
        if (customerId <= 0 || bookingId <= 0 || guests <= 0)
            return BookingChangeStatus.InvalidInput;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        // Samma grundprincip som när vi skapar bokningar:
        // lås rummet först, så att flera ändringar inte krockar.
        await using var transaction =
            await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        const string findRoomSql = """
            SELECT room_id
            FROM booking
            WHERE id = @bookingId AND customer_id = @customerId;
            """;

        int roomId;
        await using (var command = new MySqlCommand(findRoomSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@bookingId", bookingId);
            command.Parameters.AddWithValue("@customerId", customerId);
            object? roomValue = await command.ExecuteScalarAsync();

            if (roomValue is null || roomValue is DBNull)
            {
                await transaction.RollbackAsync();
                return BookingChangeStatus.BookingNotFound;
            }

            roomId = Convert.ToInt32(roomValue);
        }

        // Lås samma rumsrad som CreateBookingAsync använder FOR UPDATE på.
        const string lockRoomSql = """
            SELECT max_guests
            FROM room
            WHERE id = @roomId
            FOR UPDATE;
            """;

        int maxGuests;
        await using (var command = new MySqlCommand(lockRoomSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@roomId", roomId);
            object? capacityValue = await command.ExecuteScalarAsync();

            if (capacityValue is null || capacityValue is DBNull)
            {
                await transaction.RollbackAsync();
                return BookingChangeStatus.BookingNotFound;
            }

            maxGuests = Convert.ToInt32(capacityValue);
        }

        // Kontrollera bokningen en gång till efter låsningen.
        // Den kan ha hunnit avbokas sedan den första läsningen.
        const string lockBookingSql = """
            SELECT id
            FROM booking
            WHERE id = @bookingId
              AND customer_id = @customerId
              AND room_id = @roomId
            FOR UPDATE;
            """;

        await using (var command = new MySqlCommand(lockBookingSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@bookingId", bookingId);
            command.Parameters.AddWithValue("@customerId", customerId);
            command.Parameters.AddWithValue("@roomId", roomId);
            object? bookingValue = await command.ExecuteScalarAsync();

            if (bookingValue is null || bookingValue is DBNull)
            {
                await transaction.RollbackAsync();
                return BookingChangeStatus.BookingNotFound;
            }
        }

        if (guests > maxGuests)
        {
            await transaction.RollbackAsync();
            return BookingChangeStatus.CapacityExceeded;
        }

        const string updateSql = """
            UPDATE booking
            SET number_of_guests = @guests
            WHERE id = @bookingId
              AND customer_id = @customerId;
            """;

        await using (var command = new MySqlCommand(updateSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@guests", guests);
            command.Parameters.AddWithValue("@bookingId", bookingId);
            command.Parameters.AddWithValue("@customerId", customerId);
            // Noll ändrade rader kan också betyda att gästantalet redan var rätt.
            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return BookingChangeStatus.Updated;
    }

    public async Task<BookingChangeStatus> CancelBookingAsync(
        int customerId, int bookingId)
    {
        if (customerId <= 0 || bookingId <= 0)
            return BookingChangeStatus.InvalidInput;

        // En DELETE-sats är atomär. Vi lagrar ingen separat "upptagen"-flagga:
        // rummet blir ledigt igen när bokningen inte längre blockerar datumen.
        const string sql = """
            DELETE FROM booking
            WHERE id = @bookingId AND customer_id = @customerId;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@bookingId", bookingId);
        command.Parameters.AddWithValue("@customerId", customerId);

        int affectedRows = await command.ExecuteNonQueryAsync();
        return affectedRows == 1
            ? BookingChangeStatus.Cancelled
            : BookingChangeStatus.BookingNotFound;
    }
}
