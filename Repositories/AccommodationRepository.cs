
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
        int guests,
        bool requirePool,
        bool requireRestaurant,
        int? maxBeachDistance,
        int? maxCenterDistance,
        string sortBy)
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
            JOIN accommodation a
                ON r.accommodation_id = a.id
            LEFT JOIN facility f
                ON a.id = f.accommodation_id
            WHERE a.city = @city
                AND r.max_guests >= @guests
                AND (@pool = FALSE OR f.has_pool = TRUE)
                AND (@restaurant = FALSE OR f.has_restaurant = TRUE)
                AND (@maxBeach IS NULL
                    OR a.distance_to_beach_m <= @maxBeach)
                AND (@maxCenter IS NULL
                    OR a.distance_to_center_m <= @maxCenter)
                AND NOT EXISTS (
                    SELECT 1
                    FROM booking b
                    WHERE b.room_id = r.id
                        AND b.start_date < @checkOut
                        AND b.end_date > @checkIn
                )
            ORDER BY
                CASE WHEN @sortBy = 'price'
                    THEN r.price_per_night END ASC,
                CASE WHEN @sortBy = 'rating'
                    THEN a.rating END DESC,
                r.id ASC;
            """;

        await using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@city", city);
        command.Parameters.AddWithValue("@checkIn", checkIn);
        command.Parameters.AddWithValue("@checkOut", checkOut);
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@pool", requirePool);
        command.Parameters.AddWithValue("@restaurant", requireRestaurant);
        command.Parameters.AddWithValue(
            "@maxBeach", (object?)maxBeachDistance ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "@maxCenter", (object?)maxCenterDistance ?? DBNull.Value);
        command.Parameters.AddWithValue("@sortBy", sortBy);

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
