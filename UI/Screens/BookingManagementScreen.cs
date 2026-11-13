
using HolidayMaker.Models;
using HolidayMaker.Repositories;
using MySqlConnector;
using System.Linq;

namespace HolidayMaker.UI.Screens;

// Sköter inmatning och utskrift för ändring och avbokning.
// SQL-koden ligger i BookingManagementRepository.
public class BookingManagementScreen
{
    private readonly BookingOverviewRepository _overviewRepository;
    private readonly BookingManagementRepository _managementRepository;

    public BookingManagementScreen(string connectionString)
    {
        _overviewRepository = new BookingOverviewRepository(connectionString);
        _managementRepository = new BookingManagementRepository(connectionString);
    }

    public async Task ChangeGuestsAsync()
    {
        Console.Clear();
        Console.WriteLine("===== ÄNDRA ANTAL GÄSTER =====");
        Console.WriteLine("Skriv 0 för att gå tillbaka till huvudmenyn.\n");

        int? customerId = ReadNumber("Ange kundnummer: ");

        if (customerId == null)
        {
            Console.WriteLine("\nÄndringen avbröts.");
            return;
        }

        try
        {
            var bookings = await GetAndShowBookingsAsync(customerId.Value);

            if (bookings.Count == 0)
                return;

            int? bookingId = ReadBookingId(bookings);

            if (bookingId == null)
            {
                Console.WriteLine("\nÄndringen avbröts.");
                return;
            }

            int? guests = ReadNumber("Nytt antal gäster: ");

            if (guests == null)
            {
                Console.WriteLine("\nÄndringen avbröts.");
                return;
            }

            var result = await _managementRepository
                .UpdateGuestsAsync(customerId.Value, bookingId.Value, guests.Value);

            switch (result)
            {
                case BookingChangeStatus.Updated:
                    Console.WriteLine("\nAntalet gäster har uppdaterats!");
                    break;

                case BookingChangeStatus.CapacityExceeded:
                    Console.WriteLine(
                        "\nAntalet gäster överstiger rummets kapacitet.");
                    break;

                case BookingChangeStatus.BookingNotFound:
                    Console.WriteLine(
                        "\nBokningen finns inte längre för kunden.");
                    break;

                default:
                    Console.WriteLine(
                        "\nKunde inte ändra bokningen. Kontrollera uppgifterna.");
                    break;
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine(
                "\nEtt databasfel uppstod. Ändringen kunde inte sparas.");
        }
    }

    public async Task CancelAsync()
    {
        Console.Clear();
        Console.WriteLine("===== AVBOKA BOKNING =====");
        Console.WriteLine("Skriv 0 för att gå tillbaka till huvudmenyn.\n");

        int? customerId = ReadNumber("Ange kundnummer: ");

        if (customerId == null)
        {
            Console.WriteLine("\nAvbokningen avbröts.");
            return;
        }

        try
        {
            var bookings = await GetAndShowBookingsAsync(customerId.Value);

            if (bookings.Count == 0)
                return;

            int? bookingId = ReadBookingId(bookings);

            if (bookingId == null)
            {
                Console.WriteLine("\nAvbokningen avbröts.");
                return;
            }

            // Bekräfta avbokning
            while (true)
            {
                Console.Write(
                    $"Är du säker på att du vill avboka bokning {bookingId.Value}? (j/n): ");

                string? confirmation =
                    Console.ReadLine()?.Trim().ToLowerInvariant();

                if (confirmation == "0" || confirmation == null)
                {
                    Console.WriteLine("\nAvbokningen avbröts.");
                    return;
                }

                if (confirmation == "n")
                {
                    Console.WriteLine(
                        "\nAvbokningen avbröts. Inget har ändrats.");
                    return;
                }

                if (confirmation == "j")
                    break;

                Console.WriteLine("Ange j eller n.\n");
            }

            var result = await _managementRepository
                .CancelBookingAsync(customerId.Value, bookingId.Value);

            switch (result)
            {
                case BookingChangeStatus.Cancelled:
                    Console.WriteLine(
                        "\nBokningen är avbokad. Rummet kan sökas igen för perioden.");
                    break;

                case BookingChangeStatus.BookingNotFound:
                    Console.WriteLine(
                        "\nBokningen finns inte längre för kunden.");
                    break;

                default:
                    Console.WriteLine("\nKunde inte avboka bokningen.");
                    break;
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine(
                "\nEtt databasfel uppstod. Bokningen kunde inte avbokas.");
        }
    }

    // Hämtar och visar kundens bokningar
    private async Task<List<CustomerBooking>> GetAndShowBookingsAsync(int customerId)
    {
        var bookings = await _overviewRepository
            .GetBookingsByCustomerAsync(customerId);

        if (bookings.Count == 0)
        {
            Console.WriteLine("\nInga bokningar hittades för kunden.");
            return bookings;
        }

        Console.WriteLine("\nKundens bokningar:");

        foreach (var booking in bookings)
        {
            Console.WriteLine($"\nBokningsnummer: {booking.BookingId}");
            Console.WriteLine($"Boende: {booking.AccommodationName}");
            Console.WriteLine($"Rum: {booking.RoomNumber}");
            Console.WriteLine(
                $"Datum: {booking.StartDate:yyyy-MM-dd} till {booking.EndDate:yyyy-MM-dd}");
            Console.WriteLine($"Gäster: {booking.NumberOfGuests}");
            Console.WriteLine("------------------------------");
        }

        return bookings;
    }

    // Kontrollerar nummer och tillåter avbryt med 0
    private static int? ReadNumber(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
                return null;

            if (int.TryParse(input, out int number) && number > 0)
                return number;

            Console.WriteLine(
                "Ange ett giltigt heltal större än 0.\n");
        }
    }

    // Kontrollerar att bokningen tillhör kunden
    private static int? ReadBookingId(List<CustomerBooking> bookings)
    {
        while (true)
        {
            Console.Write("\nAnge bokningsnummer: ");
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
                return null;

            if (int.TryParse(input, out int enteredBookingId) &&
                bookings.Any(b => b.BookingId == enteredBookingId))
            {
                return enteredBookingId;
            }

            Console.WriteLine(
                "Bokningen finns inte i kundens lista. Försök igen.");
        }
    }
}
