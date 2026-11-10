using MySqlConnector;

namespace HolidayMaker.UI;

public class ConsoleMenu
{
    private readonly string _connectionString;

    public ConsoleMenu(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("===== HOLIDAY MAKER =====");
            Console.WriteLine("1. View accommodations");
            Console.WriteLine("0. Exit");
            Console.Write("\nChoose an option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    await ShowAccommodationsAsync();

                    Console.WriteLine("\nPress Enter to return to menu...");
                    Console.ReadLine();
                    break;

                case "0":
                    Console.WriteLine("Goodbye!");
                    return;

                default:
                    Console.WriteLine("\nInvalid option. Try again.");
                    Console.ReadLine();
                    break;
            }
        }
    }

    private async Task ShowAccommodationsAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

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
    }
}