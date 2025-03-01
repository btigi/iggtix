using Microsoft.Extensions.Configuration;
using System.Data.SQLite;

namespace iggtix.Services
{
    public class DB(IConfiguration config) : IDB
    {
        private IConfiguration config { get; init; } = config;

        public async Task<bool> InitializeDatabase()
        {
            var dbPath = config.GetValue<string>("DbPath");

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();
            var createCommandsTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Commands (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Trigger TEXT NOT NULL,
                        Response TEXT NOT NULL,
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UNIQUE (Trigger)
                    )";
            using var commandsCommand = new SQLiteCommand(createCommandsTableQuery, connection);
            await commandsCommand.ExecuteNonQueryAsync();

            await connection.CloseAsync();
            return true;
        }

        public async Task<bool> AddCommand(string trigger, string response)
        {
            var dbPath = config.GetValue<string>("DbPath");
            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();
            //TODO: check unique?
            var addCommandQuery = @"
                    INSERT INTO Commands (Trigger, Response)
                    VALUES (@trigger, @response)";
            using var commandsCommand = new SQLiteCommand(addCommandQuery, connection);
            commandsCommand.Parameters.AddWithValue("@trigger", trigger);
            commandsCommand.Parameters.AddWithValue("@response", response);
            await commandsCommand.ExecuteNonQueryAsync();
            await connection.CloseAsync();
            return true;
        }

        public async Task<bool> DeleteCommand(string trigger)
        {
            var dbPath = config.GetValue<string>("DbPath");
            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();
            var addCommandQuery = @"DELETE FROM Commands WHERE Trigger = @trigger";
            using var addCommand = new SQLiteCommand(addCommandQuery, connection);
            addCommand.Parameters.AddWithValue("@response", trigger);
            await addCommand.ExecuteNonQueryAsync();
            await connection.CloseAsync();
            return true;
        }

        public async Task<string> GetCommand(string trigger)
        {
            var dbPath = config.GetValue<string>("DbPath");
            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();
            var addCommandQuery = @"SELECT Response FROM Commands WHERE Trigger = @trigger";
            using var addCommand = new SQLiteCommand(addCommandQuery, connection);
            addCommand.Parameters.AddWithValue("@trigger", trigger);
            var dbResult = await addCommand.ExecuteScalarAsync();
            var result = dbResult == DBNull.Value ? "" : dbResult.ToString();
            await connection.CloseAsync();
            return result;
        }
    }
}