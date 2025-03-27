using IGDB;
using IGDB.Models;
using iggtix.Interface;
using MiniTwitch.Irc.Models;

namespace iggtix.Igdb
{
    public class Handler : IIggtixCommand
    {
        public PluginType PluginType => PluginType.Command;

        public string Name
        {
            get
            {
                return "igdb";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory, ITwitchApi twitchApi)
        {
            var clientId = Environment.GetEnvironmentVariable("iggtix_igdb_clientid");
            var clientSecret = Environment.GetEnvironmentVariable("iggtix_igdb_clientsecret");

            var msg  = message.Content.Split(" ");
            var gameName = string.Join(" ", msg.Skip(1));
            try
            {
                var api = new IGDBClient(clientId, clientSecret);

                var games = await api.QueryAsync<Game>(
                    IGDBClient.Endpoints.Games,
                    query: $"fields id,name,summary; where name ~ \"{gameName}\";"
                );

                var game = games.FirstOrDefault();
                if (game != null)
                {
                    return $"{game.Name}: {game.Summary}";
                }               
            }
            catch { }

            return string.Empty;
        }
    }
}