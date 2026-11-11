using HolidayMaker.Models;
using HolidayMaker.Repositories;
using MySqlConnector;

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
        Console.WriteLine("===== ÄNDRA ANTAL GÄSTER =====\n");

        if (!TryReadCustomerId(out int customerId))
            return;

        try
        {
            var bookings = await GetAndShowBookingsAsync(customerId);
            if (bookings.Count == 0)
                return;

            if (!TryReadBookingId(bookings, out int bookingId))
                return;

            Console.Write("Nytt antal gäster: ");
            if (!int.TryParse(Console.ReadLine(), out int guests) || guests <= 0)
            {
                Console.WriteLine("\nAnge ett giltigt antal gäster (minst 1).");
                return;
            }

            var result = await _managementRepository
                .UpdateGuestsAsync(customerId, bookingId, guests);

            switch (result)
            {
                case BookingChangeStatus.Updated:
                    Console.WriteLine("\nAntalet gäster har uppdaterats!");
                    break;
                case BookingChangeStatus.CapacityExceeded:
                    Console.WriteLine("\nAntalet gäster överstiger rummets kapacitet.");
                    break;
                case BookingChangeStatus.BookingNotFound:
                    Console.WriteLine("\nBokningen finns inte längre för kunden.");
                    break;
                default:
                    Console.WriteLine("\nKunde inte ändra bokningen. Kontrollera uppgifterna.");
                    break;
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine("\nEtt databasfel uppstod. Ändringen kunde inte sparas.");
        }
    }

    public async Task CancelAsync()
    {
        Console.WriteLine("===== AVBOKA BOKNING =====\n");

        if (!TryReadCustomerId(out int customerId))
            return;

        try
        {
            var bookings = await GetAndShowBookingsAsync(customerId);
            if (bookings.Count == 0)
                return;

            if (!TryReadBookingId(bookings, out int bookingId))
                return;

            Console.Write($"Är du säker på att du vill avboka bokning {bookingId}? (j/n): ");
            string confirmation = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

            if (confirmation != "j")
            {
                Console.WriteLine("\nAvbokningen avbröts. Inget har ändrats.");
                return;
            }

            var result = await _managementRepository
                .CancelBookingAsync(customerId, bookingId);

            switch (result)
            {
                case BookingChangeStatus.Cancelled:
                    Console.WriteLine("\nBokningen är avbokad. Rummet kan sökas igen för perioden.");
                    break;
                case BookingChangeStatus.BookingNotFound:
                    Console.WriteLine("\nBokningen finns inte längre för kunden.");
                    break;
                default:
                    Console.WriteLine("\nKunde inte avboka bokningen.");
                    break;
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine("\nEtt databasfel uppstod. Bokningen kunde inte avbokas.");
        }
    }

    private async Task<List<CustomerBooking>> GetAndShowBookingsAsync(int customerId)
    {
        var bookings = await _overviewRepository.GetBookingsByCustomerAsync(customerId);

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
            Console.WriteLine($"Datum: {booking.StartDate:yyyy-MM-dd} till {booking.EndDate:yyyy-MM-dd}");
            Console.WriteLine($"Gäster: {booking.NumberOfGuests}");
            Console.WriteLine("------------------------------");
        }

        return bookings;
    }

    private static bool TryReadCustomerId(out int customerId)
    {
        Console.Write("Ange kundnummer: ");
        if (int.TryParse(Console.ReadLine(), out customerId) && customerId > 0)
            return true;

        Console.WriteLine("\nOgiltigt kundnummer.");
        return false;
    }

    private static bool TryReadBookingId(List<CustomerBooking> bookings, out int bookingId)
    {
        Console.Write("\nAnge bokningsnummer: ");

        // Använd en lokal variabel i lambdan, inte out-parametern.
        if (int.TryParse(Console.ReadLine(), out int enteredBookingId) &&
            bookings.Any(b => b.BookingId == enteredBookingId))
        {
            bookingId = enteredBookingId;
            return true;
        }

        bookingId = 0;
        Console.WriteLine("\nBokningen finns inte i kundens lista.");
        return false;
    }
}
