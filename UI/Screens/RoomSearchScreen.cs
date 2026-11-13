
using HolidayMaker.Repositories;
using MySqlConnector;

namespace HolidayMaker.UI.Screens;

public class RoomSearchScreen
{
    private readonly AccommodationRepository _accommodationRepository;

    public RoomSearchScreen(string connectionString)
    {
        _accommodationRepository = new AccommodationRepository(connectionString);
    }

    public async Task SearchAsync()
    {
        Console.WriteLine("===== SÖK LEDIGA RUM =====");
        Console.WriteLine("Bokningsperiod: december 2026");
        Console.WriteLine("Skriv 0 för att gå tillbaka till huvudmenyn.\n");

        // Kontrollera ort
        string? city = ReadRequiredText("Ort: ");

        if (city == null)
            return;

        // Kontrollera incheckning
        // Senast 30 december eftersom utcheckning måste vara senare
        int? checkInDay = ReadInteger(
            "\nIncheckning - december 2026 (dag 1-30): ", 1, 30);

        if (checkInDay == null)
            return;

        // Kontrollera utcheckning
        int? checkOutDay = ReadInteger(
            "Utcheckning - december 2026 (dag 1-31): ",
            checkInDay.Value + 1, 31);

        if (checkOutDay == null)
            return;

        // Kontrollera antal gäster
        int? guests = ReadInteger("Antal gäster: ", 1);

        if (guests == null)
            return;

        DateTime checkIn = new(2026, 12, checkInDay.Value);
        DateTime checkOut = new(2026, 12, checkOutDay.Value);

        // Kontrollera faciliteter
        string? poolInput = ReadYesNo("\nPool krävs? (j/n): ");

        if (poolInput == null)
            return;

        string? restaurantInput = ReadYesNo(
            "Restaurang krävs? (j/n): ");

        if (restaurantInput == null)
            return;

        bool requirePool = poolInput == "j";
        bool requireRestaurant = restaurantInput == "j";

        // Kontrollera avstånd
        if (!TryReadDistance(
                "\nMaxavstånd till stranden i meter (Enter = inget krav): ",
                out int? maxBeachDistance))
            return;

        if (!TryReadDistance(
                "Maxavstånd till centrum i meter (Enter = inget krav): ",
                out int? maxCenterDistance))
            return;

        // Kontrollera sortering
        Console.WriteLine("\nSortera efter:");
        Console.WriteLine("1. Lägsta pris");
        Console.WriteLine("2. Högsta betyg");

        int? sortChoice = ReadInteger("Välj sortering: ", 1, 2);

        if (sortChoice == null)
            return;

        string sortBy = sortChoice == 2
            ? "rating"
            : "price";

        // Sök lediga rum i databasen
        try
        {
            var rooms =
                await _accommodationRepository.SearchAvailableRoomsAsync(
                    city,
                    checkIn,
                    checkOut,
                    guests.Value,
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

    // Läser text och förhindrar tomma fält
    private static string? ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
            {
                Console.WriteLine("\nSökningen avbröts.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Fältet får inte vara tomt!\n");
                continue;
            }

            if (input.Length > 100)
            {
                Console.WriteLine("Texten är för lång!\n");
                continue;
            }

            return input;
        }
    }

    // Kontrollerar heltal och tillåtna gränser
    private static int? ReadInteger(
        string prompt, int min, int? max = null)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
            {
                Console.WriteLine("\nSökningen avbröts.");
                return null;
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
                    $"Ange ett heltal mellan {min} och {max}.\n");
            }
        }
    }

    // Kontrollerar ja eller nej
    private static string? ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input =
                Console.ReadLine()?.Trim().ToLowerInvariant();

            if (input == "0" || input == null)
            {
                Console.WriteLine("\nSökningen avbröts.");
                return null;
            }

            if (input == "j" || input == "n")
                return input;

            Console.WriteLine("Ange j eller n.\n");
        }
    }

    // Frivilligt avstånd - Enter innebär inget krav
    private static bool TryReadDistance(
        string prompt, out int? distance)
    {
        distance = null;

        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();

            if (input == "0" || input == null)
            {
                Console.WriteLine("\nSökningen avbröts.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(input))
                return true;

            if (int.TryParse(input, out int meters) && meters > 0)
            {
                distance = meters;
                return true;
            }

            Console.WriteLine(
                "Ange ett positivt avstånd i meter eller tryck Enter.\n");
        }
    }
}
