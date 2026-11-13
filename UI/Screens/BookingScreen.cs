
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
        Console.WriteLine("Bokningsperiod: december 2026");
        Console.WriteLine("Skriv 0 för att gå tillbaka till huvudmenyn.\n");

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

            // Kontrollera kundnummer
            int? customerId = ReadNumber(
                "\nAnge kundnummer: ", 1);

            if (customerId == null)
            {
                Console.WriteLine("\nBokningen avbröts.");
                return;
            }

            Console.WriteLine(
                "\nRums-ID hittar du under 'Sök lediga rum'.");

            // Kontrollera rums-ID
            int? roomId = ReadNumber(
                "Ange rums-ID: ", 1);

            if (roomId == null)
            {
                Console.WriteLine("\nBokningen avbröts.");
                return;
            }

            // Kontrollera incheckning
            int? checkInDay = ReadNumber(
                "\nIncheckning - december 2026 (dag 1-30): ",
                1, 30);

            if (checkInDay == null)
            {
                Console.WriteLine("\nBokningen avbröts.");
                return;
            }

            // Kontrollera utcheckning
            int? checkOutDay = ReadNumber(
                "Utcheckning - december 2026 (dag 1-31): ",
                checkInDay.Value + 1, 31);

            if (checkOutDay == null)
            {
                Console.WriteLine("\nBokningen avbröts.");
                return;
            }

            // Kontrollera antal gäster
            int? guests = ReadNumber(
                "Antal gäster: ", 1);

            if (guests == null)
            {
                Console.WriteLine("\nBokningen avbröts.");
                return;
            }

            DateTime checkIn =
                new(2026, 12, checkInDay.Value);

            DateTime checkOut =
                new(2026, 12, checkOutDay.Value);

            // Skapa bokningen i databasen
            var result =
                await _bookingRepository.CreateBookingAsync(
                    customerId.Value,
                    roomId.Value,
                    checkIn,
                    checkOut,
                    guests.Value);

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
                        $"Antal gäster: {guests.Value}");

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
        Console.WriteLine("===== KUNDENS BOKNINGAR =====");
        Console.WriteLine("Skriv 0 för att gå tillbaka till huvudmenyn.\n");

        int? customerId = ReadNumber(
            "Ange kundnummer: ", 1);

        if (customerId == null)
        {
            Console.WriteLine("\nVisningen avbröts.");
            return;
        }

        try
        {
            var bookings = await _bookingOverviewRepository
                .GetBookingsByCustomerAsync(customerId.Value);

            if (bookings.Count == 0)
            {
                Console.WriteLine(
                    "\nInga bokningar hittades för kunden.");
                return;
            }

            foreach (var booking in bookings)
            {
                Console.WriteLine(
                    $"\nBokningsnummer: {booking.BookingId}");
                Console.WriteLine(
                    $"Boende: {booking.AccommodationName}");
                Console.WriteLine(
                    $"Rumsnummer: {booking.RoomNumber}");
                Console.WriteLine(
                    $"Incheckning: {booking.StartDate:yyyy-MM-dd}");
                Console.WriteLine(
                    $"Utcheckning: {booking.EndDate:yyyy-MM-dd}");
                Console.WriteLine(
                    $"Antal gäster: {booking.NumberOfGuests}");
                Console.WriteLine("------------------------------");
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine(
                "\nKunde inte hämta kundens bokningar.");
        }
    }

    // Läser och kontrollerar nummer
    // 0 = avbryt
    private static int? ReadNumber(
        string prompt, int min, int? max = null)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine(
                    "Du måste ange ett nummer!\n");
                continue;
            }

            if (int.TryParse(input, out int number) &&
                number >= min &&
                (max == null || number <= max.Value))
            {
                return number;
            }

            if (max == null)
            {
                Console.WriteLine(
                    $"Ange ett heltal på minst {min}.\n");
            }
            else
            {
                Console.WriteLine(
                    $"Ange ett heltal mellan {min} och {max.Value}.\n");
            }
        }
    }
}
