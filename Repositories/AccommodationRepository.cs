using MySqlConnector;

namespace HolidayMaker.Repositories;

// Används av RoomSearchScreen för att visa resultat från rumssökningen.
public sealed record AvailableRoom(
    int Id,
    string Accommodation,
    string City,
    string RoomNumber,
    int MaxGuests,
    decimal PricePerNight,
    decimal Rating);

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

        // ORDER BY kan inte parametriseras som ett vanligt värde.
        // Därför väljer vi enbart mellan två fasta, tillåtna sorteringar.
        string orderBy = sortBy == "rating"
            ? "a.rating DESC, r.price_per_night ASC"
            : "r.price_per_night ASC, a.rating DESC";

        string sql = $"""
            SELECT
                r.id,
                a.name,
                a.city,
                CAST(r.room_number AS CHAR),
                r.max_guests,
                r.price_per_night,
                a.rating
            FROM room AS r
            INNER JOIN accommodation AS a
                ON r.accommodation_id = a.id
            LEFT JOIN facility AS f
                ON f.accommodation_id = a.id
            WHERE a.city LIKE @city
              AND r.max_guests >= @guests
              AND (@requirePool = FALSE OR COALESCE(f.has_pool, FALSE) = TRUE)
              AND (@requireRestaurant = FALSE OR COALESCE(f.has_restaurant, FALSE) = TRUE)
              AND (@maxBeachDistance IS NULL
                   OR a.distance_to_beach_m <= @maxBeachDistance)
              AND (@maxCenterDistance IS NULL
                   OR a.distance_to_center_m <= @maxCenterDistance)
              AND NOT EXISTS (
                  SELECT 1
                  FROM booking AS b
                  WHERE b.room_id = r.id
                    AND b.start_date < @checkOut
                    AND b.end_date > @checkIn
              )
            ORDER BY {orderBy};
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@city", $"%{city.Trim()}%");
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@requirePool", requirePool);
        command.Parameters.AddWithValue("@requireRestaurant", requireRestaurant);
        command.Parameters.AddWithValue("@maxBeachDistance", (object?)maxBeachDistance ?? DBNull.Value);
        command.Parameters.AddWithValue("@maxCenterDistance", (object?)maxCenterDistance ?? DBNull.Value);
        command.Parameters.AddWithValue("@checkIn", checkIn);
        command.Parameters.AddWithValue("@checkOut", checkOut);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rooms.Add(new AvailableRoom(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt32(4),
                Convert.ToDecimal(reader.GetValue(5)),
                reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6))));
        }

        return rooms;
    }
}
