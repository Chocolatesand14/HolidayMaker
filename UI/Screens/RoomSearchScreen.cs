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

        if (!MenuInput.TryReadDistance(
                "\nMaxavstånd till stranden i meter (Enter = inget krav): ",
                out int? maxBeachDistance) ||
            !MenuInput.TryReadDistance(
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
}
