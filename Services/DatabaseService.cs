using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MLNetDBot.Attributes;
using MLNetDBot.EFDatabase;
using MLNetDBot.EFDatabase.EFModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MLNetDBot.Services
{
    [SetService]
    public class DatabaseService
    {
        private readonly DiscordSocketClient _client;

        public DatabaseService(DiscordSocketClient client)
        {
            _client = client;
        }

        public void BeginDatabaseServicing()
        {
            _client.Ready += EnsureDbIsCreated;
        }

        private async Task EnsureDbIsCreated()
        {
            var context = new DataContext();
            await context.Database.MigrateAsync();
            if (!context.MlMessages.Any())
            {
                await SeedDatabase(context);
                Console.WriteLine("Database has seeded");
            }
            Console.WriteLine("Ensured database creation");
        }

        private async Task SeedDatabase(DataContext context)
        {
            var seedMessage = new MlMessage { Message = "Stfu you cock", Toxic = true };
            await context.MlMessages.AddAsync(seedMessage);
            await context.SaveChangesAsync();
        }
    }
}
