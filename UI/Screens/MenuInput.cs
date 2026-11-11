namespace HolidayMaker.UI.Screens;

internal static class MenuInput
{
    public static bool TryReadDistance(
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
