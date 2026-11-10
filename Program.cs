
using MySqlConnector;
using Microsoft.Extensions.Configuration;

// Read the connection string from User Secrets.
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var connectionString = configuration["ConnectionStrings:HolidayMaker"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Database connection settings are missing.");
    return;
}

try
{
    // Connect to MySQL.
    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();

    Console.WriteLine("Connected to Holiday Maker!");

    // Retrieve accommodations from the database.
    const string sql = """
        SELECT id, name, city
        FROM accommodation
        ORDER BY id;
        """;

    await using var command = new MySqlCommand(sql, connection);
    await using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        int id = reader.GetInt32(0);
        string name = reader.GetString(1);
        string city = reader.GetString(2);

        Console.WriteLine($"{id}. {name} - {city}");
    }
}
catch (MySqlException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}

