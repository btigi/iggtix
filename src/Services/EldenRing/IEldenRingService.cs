using MiniTwitch.Irc.Models;

namespace iggtix.Services.EldenRing
{
    public interface IEldenRingService
    {
        Task<string> Handle(Privmsg message);
    }
}