using HolidayMaker.UI.Screens;

namespace HolidayMaker.UI;

public class ConsoleMenu
{
    private readonly AccommodationScreen _accommodationScreen;
    private readonly CustomerScreen _customerScreen;
    private readonly RoomSearchScreen _roomSearchScreen;
    private readonly BookingScreen _bookingScreen;
    private readonly BookingManagementScreen _bookingManagementScreen;

    public ConsoleMenu(string connectionString)
    {
        _accommodationScreen = new AccommodationScreen(connectionString);
        _customerScreen = new CustomerScreen(connectionString);
        _roomSearchScreen = new RoomSearchScreen(connectionString);
        _bookingScreen = new BookingScreen(connectionString);
        _bookingManagementScreen = new BookingManagementScreen(connectionString);
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
            Console.WriteLine("5. Visa kundens bokningar");
            Console.WriteLine("6. Ändra antal gäster");
            Console.WriteLine("7. Avboka bokning");
            Console.WriteLine("0. Avsluta");

            Console.Write("\nVälj ett alternativ: ");
            string? choice = Console.ReadLine();

            if (choice == "0")
            {
                Console.WriteLine("Tack för att du använde Holiday Maker!");
                return;
            }

            Console.Clear();

            switch (choice)
            {
                case "1":
                    await _accommodationScreen.ShowAsync();
                    break;
                case "2":
                    await _customerScreen.RegisterAsync();
                    break;
                case "3":
                    await _roomSearchScreen.SearchAsync();
                    break;
                case "4":
                    await _bookingScreen.CreateAsync();
                    break;
                case "5":
                    await _bookingScreen.ShowCustomerBookingsAsync();
                    break;
                case "6":
                    await _bookingManagementScreen.ChangeGuestsAsync();
                    break;
                case "7":
                    await _bookingManagementScreen.CancelAsync();
                    break;
                default:
                    Console.WriteLine("\nOgiltigt val. Försök igen.");
                    break;
            }

            Console.WriteLine("\nTryck på Enter för att återgå till menyn...");
            Console.ReadLine();
        }
    }
}
