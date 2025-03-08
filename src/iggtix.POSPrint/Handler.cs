using ESC_POS_USB_NET.Printer;
using iggtix.Interface;
using MiniTwitch.Irc.Models;
using System.Text;

namespace iggtix.POSPrint
{
    public class Handler : IIggtixCommand
    {
        private readonly Printer printer = new("POS-80C");

        public PluginType PluginType => PluginType.AllMessages;

        public Handler()
        {
            var encodingProvider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);
        }

        public string Name
        {
            get
            {
                return "POSPrint";
            }
        }

        public async Task<string> Handle(Privmsg message, string response, IHttpClientFactory httpClientFactory, ITwitchApi twitchApi)
        {
            printer.Clear();
            printer.Append($"{message.Author.DisplayName}: {message.Content}");
            printer.PrintDocument();
            return string.Empty;
        }
    }
}