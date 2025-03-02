using iggtix.Interface;
using MiniTwitch.Irc.Models;
using System.Security.Cryptography;
using System.Text;

namespace iggtix.rfc
{
    public class Handler1 : IIggtixCommand
    {
        public string Name
        {
            get
            {
                return "rfc";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory)
        {
            var seed = GenerateSeed(message.Author.Name, DateTime.Now.Date);
            var random = new Random(seed);
            return $"STR: {RollDice(random)} DEX:{RollDice(random)} CON:{RollDice(random)} WIS:{RollDice(random)} INT:{RollDice(random)} CHA:{RollDice(random)} ";
        }

        private static int RollDice(Random random)
        {
            int total = 0;
            for (int i = 0; i < 3; i++)
            {
                total += random.Next(1, 7);
            }
            return total;
        }

        private static int GenerateSeed(string username, DateTime date)
        {
            var input = username + date.ToString("yyyyMMdd");
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToInt32(hash, 0);
        }
    }
}