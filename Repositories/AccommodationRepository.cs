
using MySqlConnector;

namespace HolidayMaker.Repositories;

public record AvailableRoom(
    int Id,
    string Accommodation,
    string City,
    string RoomNumber,
    int MaxGuests,
    decimal PricePerNight,
    int Rating
);

public class AccommodationRepository
{
    private readonly string _connectionString;

    public AccommodationRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<AvailableRoom>> SearchAvailableRoomsAsync(
        string city,
        DateTime checkIn,
        DateTime checkOut,
        int guests)
    {
        var rooms = new List<AvailableRoom>();

        await using var connection =
            new MySqlConnection(_connectionString);

        await connection.OpenAsync();

        const string sql = """
            SELECT
                r.id,
                a.name,
                a.city,
                r.room_number,
                r.max_guests,
                r.price_per_night,
                a.rating
            FROM room r
            JOIN accommodation a ON r.accommodation_id = a.id
            WHERE a.city = @city
              AND r.max_guests >= @guests
              AND NOT EXISTS (
                  SELECT 1
                  FROM booking b
                  WHERE b.room_id = r.id
                    AND b.start_date < @checkOut
                    AND b.end_date > @checkIn
              )
            ORDER BY r.price_per_night ASC, r.id;
            """;

        await using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@city", city);
        command.Parameters.AddWithValue("@checkIn", checkIn);
        command.Parameters.AddWithValue("@checkOut", checkOut);
        command.Parameters.AddWithValue("@guests", guests);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            rooms.Add(new AvailableRoom(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                reader.GetDecimal(5),
                Convert.ToInt32(reader.GetValue(6))
            ));
        }

        return rooms;
    }
}
