using iggtix.Api;
using iggtix.Services;
using iggtix.Services.Lovecheck;
using Microsoft.Extensions.Configuration;
using MiniTwitch.Irc;
using MiniTwitch.Irc.Models;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;

namespace iggtix.Bot
{
    public class Bot : IBot
    {
        public IrcClient Client { get; init; }
        public List<(string invokeName, object instance, MethodInfo method)> Plugins { get; set; }
        private readonly TwitchApiClient _twitchApi;
        private readonly IConfiguration _config;
        private readonly IDB _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string[] DefaultCommands = ["#adda", "#dela", "#add", "#del", "#userinfo", "#lovecheck"];

        public Bot(IConfiguration config, TwitchApiClient twitchApi, IDB db, IHttpClientFactory httpClientFactory)
        {
            this._config = config;
            this._twitchApi = twitchApi;
            this._db = db;
            this._httpClientFactory = httpClientFactory;

            db.InitializeDatabase();

            Client = new IrcClient(options =>
            {
                options.Username = config.GetValue<string>("username");
                options.OAuth = config.GetValue<string>("token");
            });

            Client.OnMessage += MessageEvent;
        }

        private async ValueTask MessageEvent(Privmsg message)
        {
            if (message.Content.StartsWith("#adda") && message.Author.IsMod)
            {
                var command = message.Content.Split(" ");
                var trigger = command[1];
                if (DefaultCommands.Contains(trigger, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }
                var response = string.Join(" ", command.Skip(2));
                var exists = await _db.GetCommand(trigger);
                if (exists.Count == default)
                {
                    await _db.AddCommand(trigger, string.Empty);
                }
                await _db.AddAncillary(trigger, response);
                return;
            }

            if (message.Content.StartsWith("#dela") && message.Author.IsMod)
            {
                var command = message.Content.Split(" ");
                var trigger = command[1];
                var response = string.Join(" ", command.Skip(2));
                await _db.DeleteAncillary(trigger, response);
                return;
            }

            if (message.Content.StartsWith("#add") && message.Author.IsMod)
            {
                var command = message.Content.Split(" ");
                var trigger = command[1];
                if (DefaultCommands.Contains(trigger, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }
                var response = string.Join(" ", command.Skip(2));
                await _db.AddCommand(trigger, response);
                return;
            }

            if (message.Content.StartsWith("#del") && message.Author.IsMod)
            {
                var command = message.Content.Split(" ");
                var trigger = command[1];
                await _db.DeleteCommand(trigger);
                return;
            }

            if (message.Content == "#userinfo")
            {
                var userInfo = await _twitchApi.GetUserInfo<UserInfoResponse>("iggtix");
                await message.ReplyWith(userInfo.data.First().login);
                return;
            }

            if (message.Content.StartsWith("#lovecheck"))
            {
                var svc = new LovecheckService();
                var result = await svc.Handle(message, _httpClientFactory);
                await message.ReplyWith($"{result}");
                return;
            }

            if (message.Content.StartsWith('#'))
            {
                var trigger = message.Content.Split(" ").First();
                var responses = await _db.GetCommand(trigger);
                if (responses.Count > 0)
                {
                    var seed = GenerateSeed(message.Author.Name, DateTime.Now.Date);
                    var random = new Random(seed);
                    var response = responses[random.Next(0, responses.Count)];
                    response = await RunPlugins(message, response);

                    if (response.Contains("{CHATTER}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        response = response.Replace("{chatter}", await GetChatter(), StringComparison.InvariantCultureIgnoreCase);
                    }
                    await message.ReplyWith($"{response}");
                }
                return;
            }
        }

        private static int GenerateSeed(string username, DateTime date)
        {
            var input = username + date.ToString("yyyyMMdd");
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToInt32(hash, 0);
        }

        private async Task<string> GetChatter()
        {
            var broadcasterid = _config.GetValue<string>("broadcasterid");
            var moderatorid = _config.GetValue<string>("moderatorid");
            var chatters = await _twitchApi.GetChatters<ChattersResponse>(broadcasterid, moderatorid);
            return chatters.data.First().user_name;
        }

        private async Task<string> RunPlugins(Privmsg message, string response)
        {
            try
            {
                foreach (var plugin in Plugins)
                {
                    if (response.Contains($"{{p:{plugin.invokeName}}}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var invokeResult = plugin.method.Invoke(plugin.instance, [message, response, _httpClientFactory]);
                        if (invokeResult is Task<string> task)
                        {
                            response = await task;
                            Console.WriteLine(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception calling plugins: {ex.Message}");
            }
            return response;
        }
    }
}