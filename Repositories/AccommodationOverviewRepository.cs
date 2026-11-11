using HolidayMaker.Models;
using MySqlConnector;

namespace HolidayMaker.Repositories;

public class AccommodationOverviewRepository
{
    private readonly string _connectionString;

    public AccommodationOverviewRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<AccommodationOverview>> GetAllAsync()
    {
        var accommodations = new List<AccommodationOverview>();

        const string sql = """
            SELECT id, name, city
            FROM accommodation
            ORDER BY id;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            accommodations.Add(new AccommodationOverview(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2)));
        }

        return accommodations;
    }
}
