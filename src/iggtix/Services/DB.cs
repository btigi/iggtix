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

            var ancillaryTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Ancillary (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CommandId INTEGER NOT NULL,
                        Response TEXT NOT NULL,
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";
            using var ancillaryCommand = new SQLiteCommand(ancillaryTableQuery, connection);
            await ancillaryCommand.ExecuteNonQueryAsync();

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
            addCommand.Parameters.AddWithValue("@trigger", trigger);
            await addCommand.ExecuteNonQueryAsync();
            await connection.CloseAsync();
            return true;
        }

        public async Task<List<string>> GetCommand(string trigger)
        {
            var dbPath = config.GetValue<string>("DbPath");
            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();
            var addCommandQuery = @"
                    SELECT c.Response, a.Response as AncillaryResponse
                    FROM Commands c
                    LEFT JOIN Ancillary a ON c.Id = a.CommandId
                    WHERE 
                    c.Trigger = @trigger";
            using var addCommand = new SQLiteCommand(addCommandQuery, connection);
            addCommand.Parameters.AddWithValue("@trigger", trigger);
            var reader = await addCommand.ExecuteReaderAsync();
            var results = new List<string>();
            while (reader.Read())
            {
                var ancillaryResponse = reader["AncillaryResponse"] != null && reader["AncillaryResponse"] != DBNull.Value ? reader["AncillaryResponse"].ToString() : null;
                var response = reader["Response"] != null && reader["Response"] != DBNull.Value ? reader["Response"].ToString() : string.Empty;
                results.Add(ancillaryResponse ?? response);
            }
            await connection.CloseAsync();
            return results;
        }

        public async Task<int> GetCommandId(string trigger)
        {
            var dbPath = config.GetValue<string>("DbPath");
            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();
            var addCommandQuery = @"SELECT Id FROM Commands WHERE Trigger = @trigger";
            using var addCommand = new SQLiteCommand(addCommandQuery, connection);
            addCommand.Parameters.AddWithValue("@trigger", trigger);
            var dbResult = await addCommand.ExecuteScalarAsync();
            var result = dbResult == null || dbResult == DBNull.Value ? default : Convert.ToInt32(dbResult);
            await connection.CloseAsync();
            return result;
        }

        public async Task<bool> AddAncillary(string trigger, string response)
        {
            var commandId = await GetCommandId(trigger);
            if (commandId == default)
            {
                return false;
            }

            var dbPath = config.GetValue<string>("DbPath");
            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();
            var addCommandQuery = @"
                    INSERT INTO Ancillary (CommandId, Response)
                    VALUES (@commandId, @response)";
            using var commandsCommand = new SQLiteCommand(addCommandQuery, connection);
            commandsCommand.Parameters.AddWithValue("@commandId", commandId);
            commandsCommand.Parameters.AddWithValue("@response", response);
            await commandsCommand.ExecuteNonQueryAsync();
            await connection.CloseAsync();
            return true;
        }

        public async Task<bool> DeleteAncillary(string trigger, string response)
        {
            var commandId = await GetCommandId(trigger);
            if (commandId == default)
            {
                return false;
            }

            var dbPath = config.GetValue<string>("DbPath");
            using var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            await connection.OpenAsync();
            var addCommandQuery = @"DELETE FROM Ancillary WHERE CommandId = @commandId and Response = @response";
            using var commandsCommand = new SQLiteCommand(addCommandQuery, connection);
            commandsCommand.Parameters.AddWithValue("@commandId", commandId);
            commandsCommand.Parameters.AddWithValue("@response", response);
            await commandsCommand.ExecuteNonQueryAsync();
            await connection.CloseAsync();
            return true;
        }
    }
}