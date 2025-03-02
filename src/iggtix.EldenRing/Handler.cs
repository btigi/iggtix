using iggtix.Interface;
using MiniTwitch.Irc.Models;
using System.Text.Json;

namespace iggtix.EldenRing
{
    public class Handler1 : IIggtixCommand
    {
        public string Name
        {
            get
            {
                return "EldenRingApi";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://eldenring.fanapis.com/");

                using HttpResponseMessage apiResponse = await client.GetAsync($"api/items?limit=50");

                apiResponse.EnsureSuccessStatusCode();

                var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}");

                var result = JsonSerializer.Deserialize<EldenRingApi>(jsonResponse);
                var random = new Random();
                var randomValue = random.Next(0, 50);
                var item = result.data[randomValue];

                return item.name;
            }
            catch { }

            return string.Empty;
        }
    }


    public class Handler2 : IIggtixCommand
    {
        public string Name
        {
            get
            {
                return "EldenRingApi2";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://eldenring.fanapis.com/");

                using HttpResponseMessage apiResponse = await client.GetAsync($"api/items?limit=50");

                apiResponse.EnsureSuccessStatusCode();

                var jsonResponse = await apiResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}");

                var result = JsonSerializer.Deserialize<EldenRingApi>(jsonResponse);
                var random = new Random();
                var randomValue = random.Next(0, 50);
                var item = result.data[randomValue];

                response = response.Replace($"{{p:{Name}}}", string.Empty);
                return response.Replace("{itemname}", item.name);
            }
            catch { }

            return string.Empty;
        }
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