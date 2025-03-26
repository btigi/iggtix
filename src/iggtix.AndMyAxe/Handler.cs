using iggtix.Interface;
using MiniTwitch.Irc.Models;

namespace iggtix.AndMyAxe
{
    public class Handler : IIggtixCommand
    {
        public PluginType PluginType => PluginType.NonCommandMessage;

        public string Name
        {
            get
            {
                return "AnyMyAxe";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory, ITwitchApi twitchApi)
        {
            try
            {
                if (message.Content.Split(' ').Length == 3 && message.Content.StartsWith("and my "))
                {
                    return "and my axe!";
                }
            }
            catch { }

            return string.Empty;
        }
    }
}