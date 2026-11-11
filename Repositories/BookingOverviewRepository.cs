using HolidayMaker.Models;
using MySqlConnector;

namespace HolidayMaker.Repositories;

public class BookingOverviewRepository
{
    private readonly string _connectionString;

    public BookingOverviewRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<CustomerBooking>> GetBookingsByCustomerAsync(int customerId)
    {
        var bookings = new List<CustomerBooking>();

        const string sql = """
            SELECT
                b.id,
                a.name,
                CAST(r.room_number AS CHAR),
                b.start_date,
                b.end_date,
                b.number_of_guests
            FROM booking AS b
            INNER JOIN room AS r ON b.room_id = r.id
            INNER JOIN accommodation AS a ON r.accommodation_id = a.id
            WHERE b.customer_id = @customerId
            ORDER BY b.start_date, b.id;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@customerId", customerId);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            bookings.Add(new CustomerBooking(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDateTime(3),
                reader.GetDateTime(4),
                reader.GetInt32(5)));
        }

        return bookings;
    }
}
