using iggtix.Api;
using iggtix.Services;
using iggtix.Services.Lovecheck;
using Microsoft.Extensions.Configuration;
using MiniTwitch.Irc;
using MiniTwitch.Irc.Models;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using iggtix.Interface;

namespace iggtix.Bot
{
    public class Bot : IBot
    {
        public IrcClient Client { get; init; }
        public List<(string invokeName, object instance, MethodInfo method, PluginType pluginType)> Plugins { get; set; }
        private readonly TwitchApiClient _twitchApi;
        private readonly IConfiguration _config;
        private readonly IDB _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string[] DefaultCommands = ["#adda", "#addam", "#dela", "#add", "#addm", "#del", "#userinfo", "#lovecheck"];

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
            await RunPluginsAll(message);

            var nonCommandMessageResponse = await RunPluginsNonCommand(message);
            if (!String.IsNullOrEmpty(nonCommandMessageResponse))
            {
                await message.ReplyWith(nonCommandMessageResponse);
                return;
            }

            if ((message.Content.StartsWith("#adda") || message.Content.StartsWith("#addam")) && message.Author.IsMod)
            {
                var command = message.Content.Split(" ");
                var type = command[0];
                var trigger = command[1];
                if (DefaultCommands.Contains(trigger, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }
                var response = string.Join(" ", command.Skip(2));
                var exists = await _db.GetCommand(trigger);
                if (exists.Count == default)
                {
                    await _db.AddCommand(trigger, string.Empty, type == "#addam");
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

            if ((message.Content.StartsWith("#add") || message.Content.StartsWith("#addm")) && message.Author.IsMod)
            {
                var command = message.Content.Split(" ");
                var type = command[0];
                var trigger = command[1];
                if (DefaultCommands.Contains(trigger, StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }
                var response = string.Join(" ", command.Skip(2));
                await _db.AddCommand(trigger, response, type == "#addm");
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
                    if ((responses.Any(a => a.modOnly) && message.Author.IsMod) || !responses.Any(a => a.modOnly))
                    {
                        var seed = GenerateSeed(message.Author.Name, DateTime.Now.Date);
                        var random = new Random(seed);
                        var response = responses[random.Next(0, responses.Count)];
                        var text = await RunPluginsCommand(message, response.text);

                        if (text.Contains("{CHATTER}", StringComparison.CurrentCultureIgnoreCase))
                        {
                            text = text.Replace("{chatter}", await GetChatter(), StringComparison.InvariantCultureIgnoreCase);
                        }
                        await message.ReplyWith($"{text}");
                    }
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
            var chatters = await _twitchApi.GetChatters<ChattersResponse>();
            return chatters.data.First().user_name;
        }

        private async Task<string> RunPluginsAll(Privmsg message)
        {
            var response = string.Empty;
            try
            {
                foreach (var plugin in Plugins.Where(w => w.pluginType == PluginType.AllMessages))
                {
                    var invokeResult = plugin.method.Invoke(plugin.instance, [message, response, _httpClientFactory, _twitchApi]);
                    if (invokeResult is Task<string> task)
                    {
                        response = await task;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception calling plugins: {ex.Message}");
            }
            return response;
        }

        private async Task<string> RunPluginsCommand(Privmsg message, string response)
        {
            try
            {
                foreach (var plugin in Plugins.Where(w => w.pluginType == PluginType.Command))
                {
                    if (response.Contains($"{{p:{plugin.invokeName}}}", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var invokeResult = plugin.method.Invoke(plugin.instance, [message, response, _httpClientFactory, _twitchApi]);
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

        private async Task<string> RunPluginsNonCommand(Privmsg message)
        {
            var response = string.Empty;
            try
            {
                foreach (var plugin in Plugins.Where(w => w.pluginType == PluginType.NonCommandMessage))
                {
                    var invokeResult = plugin.method.Invoke(plugin.instance, [message, response, _httpClientFactory, _twitchApi]);
                    if (invokeResult is Task<string> task)
                    {
                        response = await task;
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