using iggtix.web.Model;
using System.Data.SQLite;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/commands", async () =>
{
    var dbPath = Environment.GetEnvironmentVariable("iggtix_dbpath");
    var commands = await GetCommands(dbPath);

    if (commands.Count == 0)
    {
        return Results.Content("No data found", "text/html");
    }

    var template = await File.ReadAllTextAsync("template.html");

    var grouped = commands.GroupBy(gb => new { gb.Trigger, gb.Response });

    var sb = new StringBuilder();
    sb.AppendLine("<table><tr><th width=\"25%\">Trigger</th><th width=\"75%\">Response</th></tr>");
    var orderedCommands = grouped.OrderBy(ob => ob.Key.Trigger).ToList();
    foreach (var commandGroup in orderedCommands)
    {
        var response = commandGroup.Key.Response;
        if (commandGroup.Count() > 1)
        {
            response = "<ul>";
            response += string.Join("", commandGroup.OrderBy(ob => ob.AncillaryResponse).Select(s => $"<li>{s.AncillaryResponse}</li>"));
            response += "</ul>";
        }

        sb.AppendLine($"<tr><td>{commandGroup.Key.Trigger}</td><td>{response}</td></tr>");
    }
    sb.AppendLine("</table>");

    template = template.Replace("{table}", sb.ToString());

    return Results.Content(template, "text/html");
});

await app.RunAsync();

static async Task<List<Command>> GetCommands(string dbPath)
{
    var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
    await connection.OpenAsync();

    var selectQuery = @"
SELECT 
  c.Trigger, 
  c.Response,
  a.Response AS AncillaryResponse
FROM Commands c
LEFT OUTER JOIN Ancillary a on a.CommandId = c.Id";
    var selectCommand = new SQLiteCommand(selectQuery, connection);
    var reader = await selectCommand.ExecuteReaderAsync();

    var commands = new List<Command>();
    while (await reader.ReadAsync())
    {
        var command = new Command
        {
            Trigger = reader["Trigger"] != DBNull.Value ? (string)reader["Trigger"] : null,
            Response = reader["Response"] != DBNull.Value ? (string)reader["Response"] : null,
            AncillaryResponse = reader["AncillaryResponse"] != DBNull.Value ? (string)reader["AncillaryResponse"] : null,
        };
        commands.Add(command);
    }
    await connection.CloseAsync();

    return commands;
}