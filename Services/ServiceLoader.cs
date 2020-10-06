using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MLNetDBot.Attributes;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MLNetDBot.Services
{
    [SetService]
    public class ServiceLoader
    {
        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;

        public ServiceLoader(CommandService commandService, IServiceProvider serviceProvider)
        {
            _commandService = commandService;
            _serviceProvider = serviceProvider;
        }

        public async Task StartLoadingServices()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _serviceProvider.GetService<CommandHandlingService>().CommandHandler();
            _serviceProvider.GetService<DatabaseService>().BeginDatabaseServicing();
            _serviceProvider.GetService<MlTrainingService>().BeginMlTrainingService();
        }
    }
}
