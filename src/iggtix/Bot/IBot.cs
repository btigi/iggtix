using iggtix.Interface;
using MiniTwitch.Irc;
using System.Reflection;

namespace iggtix.Bot
{
    public interface IBot
    {
        IrcClient Client { get; init; }
        List<(string invokeName, object instance, MethodInfo method, PluginType pluginType)> Plugins { get; set; }
    }
}