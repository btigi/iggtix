using iggtix.Interface;
using MiniTwitch.Irc.Models;

namespace iggtix.MoonPhase
{
    public class Handler : IIggtixCommand
    {
        public PluginType PluginType => PluginType.Command;

        public string Name
        {
            get
            {
                return "MoonPhase";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory, ITwitchApi twitchApi)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://diversa.igi-server.link/");

                using HttpResponseMessage apiResponse = await client.GetAsync($"api/moonphase");

                apiResponse.EnsureSuccessStatusCode();

                var result = await apiResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"{result}");

                return response.Replace("{MoonPhase}", result, StringComparison.CurrentCultureIgnoreCase);
            }
            catch { }

            return string.Empty;
        }
    }
}