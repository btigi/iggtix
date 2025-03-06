using MiniTwitch.Irc.Models;

namespace iggtix.Interface
{
    public interface IIggtixCommand
    {
        PluginType PluginType { get; }
        string Name { get; }
        Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory);
    }

    public enum PluginType
    {
        None,
        Command,
        AllMessages
    }
}