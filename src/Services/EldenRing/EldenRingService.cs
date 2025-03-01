using MiniTwitch.Irc.Models;
using System.Text.Json;

namespace iggtix.Services.EldenRing
{
    public class EldenRingService : IEldenRingService
    {
        public async Task<string> Handle(Privmsg message, IHttpClientFactory httpClientFactory)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://eldenring.fanapis.com/");

                using HttpResponseMessage response = await client.GetAsync($"api/items?limit=50");

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}\n");

                var result = JsonSerializer.Deserialize<EldenRingApi>(jsonResponse);
                var random = new Random();
                var randomValue = random.Next(0, 50);
                var item = result.data[randomValue];

                return item.name;
            }
            catch { }

            return string.Empty;
        }

        public class EldenRingApi
        {
            public bool success { get; set; }
            public int count { get; set; }
            public int total { get; set; }
            public Datum[] data { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public string description { get; set; }
            public string type { get; set; }
            public string effect { get; set; }
        }
    }
}