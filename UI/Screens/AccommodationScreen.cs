using HolidayMaker.Repositories;
using MySqlConnector;

namespace HolidayMaker.UI.Screens;

public class AccommodationScreen
{
    private readonly AccommodationOverviewRepository _repository;

    public AccommodationScreen(string connectionString)
    {
        _repository = new AccommodationOverviewRepository(connectionString);
    }

    public async Task ShowAsync()
    {
        Console.WriteLine("===== VÅRA BOENDEN =====\n");

        try
        {
            var accommodations = await _repository.GetAllAsync();

            foreach (var accommodation in accommodations)
            {
                Console.WriteLine(
                    $"{accommodation.Id}. {accommodation.Name} - {accommodation.City}");
            }
        }
        catch (MySqlException)
        {
            Console.WriteLine("Kunde inte hämta boenden. Försök igen senare.");
        }
    }
}
