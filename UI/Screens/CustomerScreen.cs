using HolidayMaker.Repositories;
using MySqlConnector;

namespace HolidayMaker.UI.Screens;

public class CustomerScreen
{
    private readonly CustomerRepository _customerRepository;

    public CustomerScreen(string connectionString)
    {
        _customerRepository = new CustomerRepository(connectionString);
    }

    public async Task RegisterAsync()
    {
        Console.WriteLine("===== REGISTRERA KUND =====\n");

        Console.Write("Namn: ");
        string? name = Console.ReadLine()?.Trim();

        Console.Write("E-postadress: ");
        string? email = Console.ReadLine()?.Trim();

        Console.Write("Telefonnummer: ");
        string? phone = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phone))
        {
            Console.WriteLine(
                "\nAlla fält måste fyllas i.");
            return;
        }

        try
        {
            long customerId =
                await _customerRepository.AddCustomerAsync(
                    name, email, phone);

            Console.WriteLine(
                $"\nKunden har registrerats! Kundnummer: {customerId}");
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            Console.WriteLine(
                "\nDet finns redan en kund med denna e-postadress.");
        }
        catch (MySqlException)
        {
            Console.WriteLine(
                "\nKunde inte registrera kunden. Försök igen.");
        }
    }
}
