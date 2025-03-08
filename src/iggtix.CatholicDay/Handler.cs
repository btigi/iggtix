using iggtix.Interface;
using MiniTwitch.Irc.Models;
using System.Text.Json;

namespace iggtix.CatholicDay
{
    public class Handler1 : IIggtixCommand
    {
        public PluginType PluginType => PluginType.Command;

        public string Name
        {
            get
            {
                return "CatholicDay";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory, ITwitchApi twitchApi)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("http://calapi.inadiutorium.cz/");

                using HttpResponseMessage apiResponse = await client.GetAsync($"api/v0/en/calendars/default/today");

                apiResponse.EnsureSuccessStatusCode();

                var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}");

                var result = JsonSerializer.Deserialize<Rootobject>(jsonResponse);

                if (result?.celebrations != null && result.celebrations.Length > 0)
                {
                    return result.celebrations[0].title;
                }
            }
            catch { }

            return "Today is a normal day";
        }
    }

    public class Rootobject
    {
        public string date { get; set; }
        public string season { get; set; }
        public int season_week { get; set; }
        public Celebration[] celebrations { get; set; }
        public string weekday { get; set; }
    }

    public class Celebration
    {
        public string title { get; set; }
        public string colour { get; set; }
        public string rank { get; set; }
        public float rank_num { get; set; }
    }
}