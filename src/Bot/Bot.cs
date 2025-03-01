using Microsoft.Extensions.Configuration;
using MiniTwitch.Irc;
using MiniTwitch.Irc.Models;
using iggtix.Api;
using iggtix.Services;
using iggtix.Services.EldenRing;
using iggtix.Services.Lovecheck;

namespace iggtix.Bot
{
    public class Bot : IBot
    {
        public IrcClient Client { get; init; }
        private readonly TwitchApiClient _twitchApi;
        private readonly IConfiguration _config;
        private readonly IDB _db;
        private readonly IHttpClientFactory _httpClientFactory;

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
            if (message.Content.StartsWith("#add") && message.Author.IsMod)
            {
                var command = message.Content.Split(" ");
                var trigger = command[1];
                var response = string.Join(" ", command.Skip(1));
                await _db.AddCommand(trigger, response);
                return;
            }

            if (message.Content.StartsWith("#edit") && message.Author.IsMod)
            {

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

            if (message.Content == "#eldenringitem")
            {
                var svc = new EldenRingService();
                var result = await svc.Handle(message, _httpClientFactory);
                await message.ReplyWith($"{result}");
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
                var response = await _db.GetCommand(trigger);
                if (!string.IsNullOrEmpty(response))
                {
                    if (response.ToUpper().Contains("{CHATTER}"))
                    {
                        response = response.Replace("{chatter}", await GetChatter(), StringComparison.InvariantCultureIgnoreCase);
                    }
                    await message.ReplyWith($"{response}");
                }
                return;
            }
        }

        private async Task<string> GetChatter()
        {
            var broadcasterid = _config.GetValue<string>("broadcasterid");
            var moderatorid = _config.GetValue<string>("moderatorid");
            var chatters = await _twitchApi.GetChatters<ChattersResponse>(broadcasterid, moderatorid);

            return chatters.data.First().user_name;
        }
    }
}