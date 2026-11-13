
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

        string? name;
        do
        {
            Console.Write("Namn: ");
            name = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Du måste ange ett namn!\n");
            }

        } while (string.IsNullOrWhiteSpace(name));

        string? email;
        do
        {
            Console.Write("E-postadress: ");
            email = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Du måste ange en e-postadress!\n");
            }

        } while (string.IsNullOrWhiteSpace(email));

        string? phone;
        do
        {
            Console.Write("Telefonnummer: ");
            phone = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(phone))
            {
                Console.WriteLine("Du måste ange ett telefonnummer!\n");
            }

        } while (string.IsNullOrWhiteSpace(phone));

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
