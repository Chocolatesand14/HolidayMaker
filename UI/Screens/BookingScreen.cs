using HolidayMaker.Repositories;
using MySqlConnector;

namespace HolidayMaker.UI.Screens;

public class BookingScreen
{
    private readonly BookingRepository _bookingRepository;
    private readonly BookingOverviewRepository _bookingOverviewRepository;

    public BookingScreen(string connectionString)
    {
        _bookingRepository = new BookingRepository(connectionString);
        _bookingOverviewRepository = new BookingOverviewRepository(connectionString);
    }

    public async Task CreateAsync()
    {
        Console.WriteLine("===== SKAPA BOKNING =====");
        Console.WriteLine("Bokningsperiod: december 2026\n");

        try
        {
            var customers =
                await _bookingRepository.GetCustomersAsync();

            if (customers.Count == 0)
            {
                Console.WriteLine(
                    "Det finns inga registrerade kunder.");
                return;
            }

            Console.WriteLine("Registrerade kunder:");

            foreach (var customer in customers)
            {
                Console.WriteLine(
                    $"{customer.Id}. {customer.Name}");
            }

            Console.Write("\nAnge kundnummer: ");
            bool validCustomer = int.TryParse(
                Console.ReadLine(),
                out int customerId);

            Console.WriteLine(
                "\nRums-ID hittar du under 'Sök lediga rum'.");

            Console.Write("Ange rums-ID: ");
            bool validRoom = int.TryParse(
                Console.ReadLine(),
                out int roomId);

            Console.Write("\nIncheckning - december 2026 (dag 1-31): ");
            bool validCheckIn = int.TryParse(
                Console.ReadLine(),
                out int checkInDay);

            Console.Write("Utcheckning - december 2026 (dag 1-31): ");
            bool validCheckOut = int.TryParse(
                Console.ReadLine(),
                out int checkOutDay);

            Console.Write("Antal gäster: ");
            bool validGuests = int.TryParse(
                Console.ReadLine(),
                out int guests);

            if (!validCustomer ||
                !validRoom ||
                !validCheckIn ||
                !validCheckOut ||
                !validGuests ||
                customerId <= 0 ||
                roomId <= 0 ||
                checkInDay < 1 ||
                checkInDay > 31 ||
                checkOutDay < 1 ||
                checkOutDay > 31 ||
                checkOutDay <= checkInDay ||
                guests <= 0)
            {
                Console.WriteLine(
                    "\nOgiltiga bokningsuppgifter. Kontrollera dina val.");
                return;
            }

            DateTime checkIn = new(2026, 12, checkInDay);
            DateTime checkOut = new(2026, 12, checkOutDay);

            var result =
                await _bookingRepository.CreateBookingAsync(
                    customerId,
                    roomId,
                    checkIn,
                    checkOut,
                    guests);

            switch (result.Status)
            {
                case BookingStatus.Created:
                    Console.WriteLine(
                        "\n===== BOKNING BEKRÄFTAD =====");

                    Console.WriteLine(
                        $"Bokningsnummer: {result.BookingId}");

                    Console.WriteLine(
                        $"Incheckning: {checkIn:yyyy-MM-dd}");

                    Console.WriteLine(
                        $"Utcheckning: {checkOut:yyyy-MM-dd}");

                    Console.WriteLine(
                        $"Antal nätter: {(checkOut - checkIn).Days}");

                    Console.WriteLine(
                        $"Antal gäster: {guests}");

                    Console.WriteLine(
                        $"Totalpris: {result.TotalPrice:N2} kr");
                    break;

                case BookingStatus.CustomerNotFound:
                    Console.WriteLine(
                        "\nKunden finns inte i databasen.");
                    break;

                case BookingStatus.RoomNotFound:
                    Console.WriteLine(
                        "\nRummet finns inte i databasen.");
                    break;

                case BookingStatus.CapacityExceeded:
                    Console.WriteLine(
                        "\nAntalet gäster överstiger rummets kapacitet.");
                    break;

                case BookingStatus.RoomNotAvailable:
                    Console.WriteLine(
                        "\nRummet är redan bokat under den valda perioden.");
                    break;

                default:
                    Console.WriteLine(
                        "\nBokningen kunde inte genomföras.");
                    break;
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine(
                "\nEtt databasfel uppstod. Bokningen kunde inte genomföras.");
        }
    }

    public async Task ShowCustomerBookingsAsync()
    {
        Console.WriteLine("===== KUNDENS BOKNINGAR =====\n");
        Console.Write("Ange kundnummer: ");

        if (!int.TryParse(Console.ReadLine(), out int customerId) ||
            customerId <= 0)
        {
            Console.WriteLine("\nOgiltigt kundnummer.");
            return;
        }

        try
        {
            var bookings = await _bookingOverviewRepository
                .GetBookingsByCustomerAsync(customerId);

            if (bookings.Count == 0)
            {
                Console.WriteLine("\nInga bokningar hittades för kunden.");
                return;
            }

            foreach (var booking in bookings)
            {
                Console.WriteLine($"\nBokningsnummer: {booking.BookingId}");
                Console.WriteLine($"Boende: {booking.AccommodationName}");
                Console.WriteLine($"Rumsnummer: {booking.RoomNumber}");
                Console.WriteLine($"Incheckning: {booking.StartDate:yyyy-MM-dd}");
                Console.WriteLine($"Utcheckning: {booking.EndDate:yyyy-MM-dd}");
                Console.WriteLine($"Antal gäster: {booking.NumberOfGuests}");
                Console.WriteLine("------------------------------");
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine("\nKunde inte hämta kundens bokningar.");
        }
    }
}
