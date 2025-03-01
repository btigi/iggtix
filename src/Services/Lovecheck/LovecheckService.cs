using MiniTwitch.Irc.Models;
using Similarity;

namespace iggtix.Services.Lovecheck
{
    public class LovecheckService : ILovecheckService
    {
        public async Task<string> Handle(Privmsg message, IHttpClientFactory httpClientFactory)
        {
            var split = message.Content.Split(' ');
            if (split.Length > 1)
            {
                var l = StringSimilarity.Calculate(message.Author.Name, split[1]);
                return Convert.ToString(l);
            }            

            return string.Empty;
        }
    }
}