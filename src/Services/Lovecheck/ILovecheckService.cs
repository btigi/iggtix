using MiniTwitch.Irc.Models;

namespace iggtix.Services.Lovecheck
{
    public interface ILovecheckService
    {
        Task<string> Handle(Privmsg message, IHttpClientFactory httpClientFactory);
    }
}