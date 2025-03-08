using iggtix.Interface;
using iggtix.Interface.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace iggtix.Api
{
    public class TwitchApiClient : ITwitchApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _client;
        private readonly ILogger<TwitchApiClient> _logger;
        private readonly IConfiguration _config;

        public TwitchApiClient(IHttpClientFactory httpClientFactory, ILogger<TwitchApiClient> logger, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _client = httpClientFactory.CreateClient();
            _logger = logger;
            _config = config;

            _client.BaseAddress = new Uri("https://api.twitch.tv/");
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.GetValue<string>("token")}");
            _client.DefaultRequestHeaders.Add("Client-Id", $"{_config.GetValue<string>("clientid")}");
        }


        public async Task<ChannelInfo> GetChannelInfo()
        {
            try
            {
                var broadcasterid = _config.GetValue<string>("broadcasterid");
                using HttpResponseMessage response = await _client.GetAsync($"/helix/channels?broadcaster_id={broadcasterid}");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}\n");
                var result = System.Text.Json.JsonSerializer.Deserialize<ChannelInfo>(jsonResponse);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to Twitch API {ex}");
                throw;
            }
        }

        public async Task<T> GetChatters<T>()
        {
            try
            {
                var broadcasterid = _config.GetValue<string>("broadcasterid");
                var moderatorid = _config.GetValue<string>("moderatorid");
                using HttpResponseMessage response = await _client.GetAsync($"/helix/chat/chatters?broadcaster_id={broadcasterid}&moderator_id={moderatorid}");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}\n");
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(jsonResponse);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to Twitch API {ex}");
                throw;
            }
        }

        public async Task<T> GetUserInfo<T>(string loginName)
        {
            try
            {
                using HttpResponseMessage response = await _client.GetAsync($"helix/users?login={loginName}");
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}\n");
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(jsonResponse);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to Twitch API {ex}");
                throw;
            }
        }

        public async Task<T> RefreshToken<T>(string refreshToken)
        {
            try
            {
                var encodedToken = WebUtility.UrlEncode(refreshToken);
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://id.twitch.tv/oauth2/token");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.GetValue<string>("token")}");
                client.DefaultRequestHeaders.Add("Client-Id", $"{_config.GetValue<string>("clientid")}");
                client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

                var client_secret = ""; //TODO:

                var parameters = new Dictionary<string, string>
                {
                    { "client_id", _config.GetValue<string>("clientid") },
                    { "client_secret", client_secret },
                    { "grant_type", "refresh_token" },
                    { "refresh_token", encodedToken }
                };

                using HttpResponseMessage response = await client.PostAsync("/", new FormUrlEncodedContent(parameters));

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}\n");

                var result = System.Text.Json.JsonSerializer.Deserialize<T>(jsonResponse);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to Twitch API {ex}");
                throw;
            }
        }

        public async Task<T> ValidateToken<T>(string accessToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://id.twitch.tv/oauth2/validate");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.GetValue<string>("token")}");

                using HttpResponseMessage response = await client.GetAsync("/");

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{jsonResponse}\n");

                var result = System.Text.Json.JsonSerializer.Deserialize<T>(jsonResponse);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"An error occurred connecting to Twitch API {ex}");
                throw;
            }
        }
    }




}