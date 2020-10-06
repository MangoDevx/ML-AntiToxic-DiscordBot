#region

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MLNetDBot.Attributes;
using System;
using System.Threading.Tasks;

#endregion

// ReSharper disable UnusedMember.Global
namespace MLNetDBot.Services
{
    [SetService]
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandlingService(DiscordSocketClient client, CommandService commands,
            IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public void CommandHandler()
        {
            _client.MessageReceived += HandleCommandAsync;
        }

        public Task HandleCommandAsync(SocketMessage messageParam)
        {
            _ = Task.Run(async () =>
            {
                if (!(messageParam is SocketUserMessage message) || messageParam.Author.IsBot) return;
                var guild = (message.Channel as SocketTextChannel)?.Guild;
                if (guild is null)
                {
                    Console.WriteLine($"{messageParam.Author.ToString() ?? "error"} attempted to DM the bot: \"{messageParam.Content}\"");
                    await messageParam.Author.SendMessageAsync(
                        "I do not work in private messages. Please talk in the server, or DM support.");
                    return;
                }

                var argPos = 0;
                if (!(message.HasStringPrefix("!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix("?", ref argPos))) return;
                var context = new SocketCommandContext(_client, message);
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync(
                            $"{context.User.Mention} You did not supply the correct amount of arguments for this command.");
                        break;
                    case CommandError.Exception:
                        Console.WriteLine(result.ErrorReason);
                        break;
                }
            });
            return Task.CompletedTask;
        }
    }
}