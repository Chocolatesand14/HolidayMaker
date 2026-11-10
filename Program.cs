
using HolidayMaker.UI;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var connectionString = configuration["ConnectionStrings:HolidayMaker"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Database connection settings are missing.");
    return;
}

var menu = new ConsoleMenu(connectionString);

await menu.RunAsync();
