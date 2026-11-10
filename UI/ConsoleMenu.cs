
using MySqlConnector;
using HolidayMaker.Repositories;

namespace HolidayMaker.UI;

public class ConsoleMenu
{
    private readonly string _connectionString;
    private readonly CustomerRepository _customerRepository;

    public ConsoleMenu(string connectionString)
    {
        _connectionString = connectionString;
        _customerRepository = new CustomerRepository(connectionString);
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("===== HOLIDAY MAKER =====");
            Console.WriteLine("1. View accommodations");
            Console.WriteLine("2. Register customer");
            Console.WriteLine("0. Exit");
            Console.Write("\nChoose an option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Clear();
                    await ShowAccommodationsAsync();
                    break;

                case "2":
                    Console.Clear();
                    await RegisterCustomerAsync();
                    break;

                case "0":
                    Console.WriteLine("Goodbye!");
                    return;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine("\nPress Enter to return to menu...");
            Console.ReadLine();
        }
    }

    private async Task RegisterCustomerAsync()
    {
        Console.WriteLine("===== REGISTER CUSTOMER =====\n");

        Console.Write("Name: ");
        string? name = Console.ReadLine()?.Trim();

        Console.Write("Email: ");
        string? email = Console.ReadLine()?.Trim();

        Console.Write("Phone number: ");
        string? phone = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone))
        {
            Console.WriteLine("\nAll fields are required.");
            return;
        }

        try
        {
            long customerId = await _customerRepository.AddCustomerAsync(
                name, email, phone);

            Console.WriteLine($"\nCustomer registered! ID: {customerId}");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            Console.WriteLine("\nA customer with this email already exists.");
        }
        catch (MySqlException)
        {
            Console.WriteLine("\nCould not register customer. Check the database connection.");
        }
    }

    private async Task ShowAccommodationsAsync()
    {
        try
        {
            await using var connection =
                new MySqlConnection(_connectionString);

            await connection.OpenAsync();

            const string sql = """
                SELECT id, name, city
                FROM accommodation
                ORDER BY id;
                """;

            await using var command =
                new MySqlCommand(sql, connection);

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Console.WriteLine(
                    $"{reader.GetInt32(0)}. {reader.GetString(1)} - {reader.GetString(2)}");
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine("Could not retrieve accommodations.");
        }
    }
}
