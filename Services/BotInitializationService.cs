#region

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MLNetDBot.Attributes;
using MLNetDBot.EFDatabase;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

#endregion

namespace MLNetDBot.Services
{
    public class BotInitializationService
    {
        private readonly HttpClient _http = new HttpClient();
        private DiscordSocketClient _client;
        private CommandService _commandService;
        private IServiceCollection _serviceCollection;

        public async Task StartInitializationAsync()
        {
            var discordSocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 500,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                AlwaysDownloadUsers = true
            };

            var commandServiceConfig = new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose
            };

            _client = new DiscordSocketClient(discordSocketConfig);
            _commandService = new CommandService(commandServiceConfig);

            _serviceCollection = new ServiceCollection()
                .AddDbContext<DataContext>()
                .AddSingleton(_commandService)
                .AddSingleton(_http)
                .AddSingleton(_client);

            var service = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(y => y.GetCustomAttributes(typeof(SetServiceAttribute), true).Length > 0);
            foreach (var services in service)
                _serviceCollection.AddSingleton(services);
            var builtService = _serviceCollection.BuildServiceProvider();
            await builtService.GetService<ServiceLoader>().StartLoadingServices();

            await _client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["Token"]);
            await _client.StartAsync();

            _client.Log += client_Log;
            await Task.Delay(-1);
        }

        private Task client_Log(LogMessage arg)
        {
            Console.WriteLine($"{DateTime.UtcNow.Subtract(TimeSpan.FromHours(4))}: [{arg.Severity,-8}] {arg.Message} @ {arg.Source}");
            return Task.CompletedTask;
        }
    }
}