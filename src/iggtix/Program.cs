using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using iggtix.Api;
using iggtix.Bot;
using iggtix.Services;
using System.Reflection;
using iggtix.Interface;

var host = Host.CreateDefaultBuilder()
           .ConfigureServices(ConfigureServices)
           .ConfigureAppConfiguration((context, config) =>
           {
               config.AddEnvironmentVariables(prefix: "iggtix_");
           })
           .Build();

var bot = host
   .Services
   .GetService<IBot>();

var config = host
   .Services
   .GetService<IConfiguration>();

var pluginDirectory = config.GetValue<string>("plugindir");
var plugins = Load(pluginDirectory);
bot.Plugins = plugins;

var channel = config.GetValue<string>("channel");
await bot.Client.ConnectAsync();
await bot.Client.JoinChannel(channel);

_ = Console.ReadLine();

static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddSingleton<TwitchApiClient>();
    services.AddSingleton<IBot, Bot>();
    services.AddSingleton<IDB, DB>();
    services.AddHttpClient();
}

static List<(string invokeName, object instance, MethodInfo method, PluginType pluginType)> Load(string pluginDirectory)
{
    var results = new List<(string invokeName, object instance, MethodInfo method, PluginType pluginType)>();
    try
    {
        foreach (var plugin in Directory.EnumerateFiles(pluginDirectory, "*.dll"))
        {
            var assembly = Assembly.LoadFrom(plugin);
            var interfaceType = typeof(IIggtixCommand);
            var classTypes = assembly.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface);

            if (classTypes != null)
            {
                foreach (var classType in classTypes)
                {
                    var NameProperty = classType.GetProperty("Name");
                    var pluginTypeProperty = classType.GetProperty("PluginType");
                    var method = classType.GetMethod("Handle");
                    var instance = Activator.CreateInstance(classType);
                    var name = NameProperty.GetValue(instance);
                    var pluginType = pluginTypeProperty.GetValue(instance);
                    Console.WriteLine(name);
                    results.Add(((string)name, instance, method, (PluginType)pluginType));
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception loading plugins: {ex.Message}");
    }
    return results;
}