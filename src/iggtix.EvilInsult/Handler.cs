using iggtix.Interface;
using MiniTwitch.Irc.Models;
using System.Text.Json;

namespace iggtix.EvilInsult
{
    public class Handler1 : IIggtixCommand
    {
        public PluginType PluginType => PluginType.Command;

        public string Name
        {
            get
            {
                return "EvilInsult";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory, ITwitchApi twitchApi)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://evilinsult.com/");

                using HttpResponseMessage apiResponse = await client.GetAsync($"generate_insult.php?lang=en&type=json");

                apiResponse.EnsureSuccessStatusCode();

                var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}");

                var result = JsonSerializer.Deserialize<Rootobject>(jsonResponse);

                return result.insult;
            }
            catch { }

            return string.Empty;
        }
    }
    public class Rootobject
    {
        public string number { get; set; }
        public string language { get; set; }
        public string insult { get; set; }
        public string created { get; set; }
        public string shown { get; set; }
        public string createdby { get; set; }
        public string active { get; set; }
        public string comment { get; set; }
    }
}