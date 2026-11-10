
using MySqlConnector;
using HolidayMaker.Repositories;

namespace HolidayMaker.UI;

public class ConsoleMenu
{
    private readonly string _connectionString;
    private readonly CustomerRepository _customerRepository;
    private readonly AccommodationRepository _accommodationRepository;
    private readonly BookingRepository _bookingRepository;

    public ConsoleMenu(string connectionString)
    {
        _connectionString = connectionString;

        _customerRepository =
            new CustomerRepository(connectionString);

        _accommodationRepository =
            new AccommodationRepository(connectionString);

        _bookingRepository =
            new BookingRepository(connectionString);
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("===== HOLIDAY MAKER =====");
            Console.WriteLine("1. Visa boenden");
            Console.WriteLine("2. Registrera kund");
            Console.WriteLine("3. Sök lediga rum");
            Console.WriteLine("4. Skapa bokning");
            Console.WriteLine("0. Avsluta");

            Console.Write("\nVälj ett alternativ: ");
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

                case "3":
                    Console.Clear();
                    await SearchRoomsAsync();
                    break;

                case "4":
                    Console.Clear();
                    await CreateBookingAsync();
                    break;

                case "0":
                    Console.WriteLine(
                        "Tack för att du använde Holiday Maker!");
                    return;

                default:
                    Console.WriteLine(
                        "\nOgiltigt val. Försök igen.");
                    break;
            }

            Console.WriteLine(
                "\nTryck på Enter för att återgå till menyn...");

            Console.ReadLine();
        }
    }

    private async Task ShowAccommodationsAsync()
    {
        Console.WriteLine("===== VÅRA BOENDEN =====\n");

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
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string city = reader.GetString(2);

                Console.WriteLine(
                    $"{id}. {name} - {city}");
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine(
                "Kunde inte hämta boenden. Försök igen senare.");
        }
    }

    private async Task RegisterCustomerAsync()
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

    private async Task SearchRoomsAsync()
    {
        Console.WriteLine("===== SÖK LEDIGA RUM =====");
        Console.WriteLine("Bokningsperiod: december 2026\n");

        Console.Write("Ort: ");
        string city = Console.ReadLine()?.Trim() ?? "";

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

        if (string.IsNullOrWhiteSpace(city) ||
            !validCheckIn ||
            !validCheckOut ||
            !validGuests ||
            checkInDay < 1 ||
            checkInDay > 31 ||
            checkOutDay < 1 ||
            checkOutDay > 31 ||
            checkOutDay <= checkInDay ||
            guests <= 0)
        {
            Console.WriteLine(
                "\nOgiltiga sökuppgifter. Kontrollera datum och antal gäster.");
            return;
        }

        DateTime checkIn = new(2026, 12, checkInDay);
        DateTime checkOut = new(2026, 12, checkOutDay);

        Console.Write("\nPool krävs? (j/n): ");
        string poolInput =
            Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

        Console.Write("Restaurang krävs? (j/n): ");
        string restaurantInput =
            Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

        if ((poolInput != "j" && poolInput != "n") ||
            (restaurantInput != "j" && restaurantInput != "n"))
        {
            Console.WriteLine(
                "\nAnge j eller n för faciliteter.");
            return;
        }

        bool requirePool = poolInput == "j";
        bool requireRestaurant = restaurantInput == "j";

        if (!TryReadDistance(
                "\nMaxavstånd till stranden i meter (Enter = inget krav): ",
                out int? maxBeachDistance) ||
            !TryReadDistance(
                "Maxavstånd till centrum i meter (Enter = inget krav): ",
                out int? maxCenterDistance))
        {
            Console.WriteLine(
                "\nAnge ett giltigt avstånd.");
            return;
        }

        Console.WriteLine("\nSortera efter:");
        Console.WriteLine("1. Lägsta pris");
        Console.WriteLine("2. Högsta betyg");
        Console.Write("Välj sortering: ");

        string? sortChoice = Console.ReadLine();

        if (sortChoice != "1" && sortChoice != "2")
        {
            Console.WriteLine(
                "\nOgiltigt val av sortering.");
            return;
        }

        string sortBy = sortChoice == "2"
            ? "rating"
            : "price";

        try
        {
            var rooms =
                await _accommodationRepository.SearchAvailableRoomsAsync(
                    city,
                    checkIn,
                    checkOut,
                    guests,
                    requirePool,
                    requireRestaurant,
                    maxBeachDistance,
                    maxCenterDistance,
                    sortBy);

            if (rooms.Count == 0)
            {
                Console.WriteLine(
                    "\nInga lediga rum hittades för dina sökkriterier.");
                return;
            }

            Console.WriteLine("\n===== TILLGÄNGLIGA RUM =====");
            Console.WriteLine(
                $"Period: {checkIn:yyyy-MM-dd} till {checkOut:yyyy-MM-dd}");

            foreach (var room in rooms)
            {
                Console.WriteLine(
                    $"\nRums-ID: {room.Id}" +
                    $"\nBoende: {room.Accommodation}" +
                    $"\nOrt: {room.City}" +
                    $"\nRumsnummer: {room.RoomNumber}" +
                    $"\nMax antal gäster: {room.MaxGuests}" +
                    $"\nPris per natt: {room.PricePerNight:N2} kr" +
                    $"\nBetyg: {room.Rating}/5");
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine(
                "\nKunde inte söka efter rum. Försök igen senare.");
        }
    }

    private async Task CreateBookingAsync()
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

    private static bool TryReadDistance(
        string prompt,
        out int? distance)
    {
        Console.Write(prompt);

        string input =
            Console.ReadLine()?.Trim() ?? "";

        distance = null;

        if (input.Length == 0)
        {
            return true;
        }

        if (!int.TryParse(input, out int value) || value < 0)
        {
            return false;
        }

        distance = value;
        return true;
    }
}
