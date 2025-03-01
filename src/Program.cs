using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using iggtix.Api;
using iggtix.Bot;
using iggtix.Services;

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

var channel = config.GetValue<string>("channel");

await bot.Client.ConnectAsync();
await bot.Client.JoinChannel(channel);

_ = Console.ReadLine();

static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddHttpClient<TwitchApiClient>();
    services.AddSingleton<IBot, Bot>();
    services.AddSingleton<IDB, DB>();
}