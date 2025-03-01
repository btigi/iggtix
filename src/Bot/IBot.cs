using MiniTwitch.Irc;

namespace iggtix.Bot
{
    public interface IBot
    {
        IrcClient Client { get; init; }
    }
}