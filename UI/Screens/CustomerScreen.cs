
using HolidayMaker.Repositories;
using MySqlConnector;
using System.Net.Mail;
using System.Linq;

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
        Console.WriteLine("===== REGISTRERA KUND =====");
        Console.WriteLine("Skriv 0 för att gå tillbaka till huvudmenyn.\n");

        // Kontrollera namn
        string name;

        while (true)
        {
            Console.Write("Namn: ");
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
            {
                Console.WriteLine("\nRegistreringen avbröts.");
                return;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Du måste ange ett namn!\n");
                continue;
            }

            if (input.Length > 100)
            {
                Console.WriteLine("Namnet är för långt!\n");
                continue;
            }

            name = input;
            break;
        }

        // Kontrollera e-postadress
        string email;

        while (true)
        {
            Console.Write("E-postadress: ");
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
            {
                Console.WriteLine("\nRegistreringen avbröts.");
                return;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Du måste ange en e-postadress!\n");
                continue;
            }

            if (input.Length > 255 ||
                input.Contains(' ') ||
                !MailAddress.TryCreate(input, out var address) ||
                address.Address != input ||
                !address.Host.Contains('.'))
            {
                Console.WriteLine("Ogiltig e-postadress. Försök igen.\n");
                continue;
            }

            email = input;
            break;
        }

        // Kontrollera telefonnummer
        string phone;

        while (true)
        {
            Console.Write("Telefonnummer: ");
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
            {
                Console.WriteLine("\nRegistreringen avbröts.");
                return;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Du måste ange ett telefonnummer!\n");
                continue;
            }

            string digits = new string(
                input.Where(char.IsDigit).ToArray());

            bool validCharacters = input.All(
                c => char.IsDigit(c) ||
                     c == ' ' || c == '-' || c == '+');

            if (input.Length > 30 ||
                !validCharacters ||
                input.Count(c => c == '+') > 1 ||
                (input.Contains('+') && !input.StartsWith('+')) ||
                digits.Length < 7 ||
                digits.Length > 15)
            {
                Console.WriteLine("Ogiltigt telefonnummer. Försök igen.\n");
                continue;
            }

            phone = input;
            break;
        }

        // Registrera kunden
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
