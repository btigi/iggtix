using MiniTwitch.Irc.Models;

namespace iggtix.Interface
{
    public interface IIggtixCommand
    {
        string Name { get; }
        Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory);
    }
}