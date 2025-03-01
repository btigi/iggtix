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
        private readonly TwitchApiClient HttpClient;
        private readonly IConfiguration Config;
        private readonly IDB Db;

        public Bot(IConfiguration config, TwitchApiClient httpClient, IDB db)
        {
            this.Config = config;
            this.HttpClient = httpClient;
            this.Db = db;

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
                await Db.AddCommand(trigger, response);
                return;
            }

            if (message.Content.StartsWith("#edit") && message.Author.IsMod)
            {

            }

            if (message.Content.StartsWith("#del") && message.Author.IsMod)
            {
                var command = message.Content.Split(" ");
                var trigger = command[1];
                await Db.DeleteCommand(trigger);
                return;
            }

            if (message.Content == "#stepdaddy")
            {
                var broadcasterid = Config.GetValue<string>("broadcasterid");
                var moderatorid = Config.GetValue<string>("moderatorid");
                var chatters = await HttpClient.GetChatters<ChattersResponse>(broadcasterid, moderatorid);

                var response = $"{message.Author}'s stepdaddy is {chatters.data.First().user_name}";

                await message.ReplyWith(response);
                return;
            }

            if (message.Content == "#userinfo")
            {
                var userInfo = await HttpClient.GetUserInfo<UserInfoResponse>("iggtix");
                await message.ReplyWith(userInfo.data.First().login);
                return;
            }


            if (message.Content == "#eldenringitem")
            {
                var svc = new EldenRingService();
                var result = await svc.Handle(message);
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
                var command = await Db.GetCommand(trigger);
                if (!string.IsNullOrEmpty(command))
                {
                    await message.ReplyWith($"{command}");
                }
                return;
            }
        }
    }
}