
using MySqlConnector;

namespace HolidayMaker.Repositories;

public class CustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> AddCustomerAsync(
        string name,
        string email,
        string phoneNumber)
    {
        await using var connection =
            new MySqlConnection(_connectionString);

        await connection.OpenAsync();

        const string sql = """
            INSERT INTO customer (name, email, phone_number)
            VALUES (@name, @email, @phoneNumber);
            """;

        await using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@phoneNumber", phoneNumber);

        await command.ExecuteNonQueryAsync();

        return command.LastInsertedId;
    }
}
