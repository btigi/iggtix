using Microsoft.Extensions.Configuration;
using MiniTwitch.Irc;
using MiniTwitch.Irc.Models;
using iggtix.Api;
using iggtix.Services;
using iggtix.Services.EldenRing;

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
                var response = command[2];
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

            if (message.Content == "#stepdaddy")
            {
                var broadcasterid = _config.GetValue<string>("broadcasterid");
                var moderatorid = _config.GetValue<string>("moderatorid");
                var chatters = await _twitchApi.GetChatters<ChattersResponse>(broadcasterid, moderatorid);

                var response = $"{message.Author}'s stepdaddy is {chatters.data.First().user_name}";

                await message.ReplyWith(response);
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

            if (message.Content == "#hello?")
            {
                await message.ReplyWith("hello <3");
                return;
            }

            if (message.Content.StartsWith('#'))
            {
                var trigger = message.Content.Split(" ").First();
                var command = await _db.GetCommand(trigger);
                if (!string.IsNullOrEmpty(command))
                {
                    await message.ReplyWith($"{command}");
                }
                return;
            }
        }
    }
}