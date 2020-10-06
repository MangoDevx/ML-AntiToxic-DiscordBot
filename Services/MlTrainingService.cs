using Discord;
using Discord.WebSocket;
using MLNetDBot.Attributes;
using MLNetDBot.EFDatabase;
using MLNetDBot.EFDatabase.EFModels;
using MLNetDBot.JsonModels;
using SampleClassification.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MLNetDBot.Services
{
    [SetService]
    public class MlTrainingService
    {
        private readonly DiscordSocketClient _client;
        private readonly DataContext _dbContext;
        private readonly List<string> _blockedContent = new List<string> { "> ", "!toxic", "cs " };

        public MlTrainingService(DiscordSocketClient client, DataContext dbContext)
        {
            _client = client;
            _dbContext = dbContext;
        }

        public void BeginMlTrainingService()
        {
            _client.ReactionAdded += MlTrainingReactionAdded;
            _client.MessageReceived += TransferToxicMessages;
            _client.MessageReceived += SubmitMessageForManualReview;
        }

        private async Task MlTrainingReactionAdded(Discord.Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel socketMessageChannel, SocketReaction socketReaction)
        {
            if (socketReaction.UserId != 386969677143736330) return;
            var dbContext = _dbContext;
            var msg = await socketMessageChannel.GetMessageAsync(socketReaction.MessageId);
            if (msg.Content.Length < 2) return;
            var sanitizedMsg = _blockedContent.Aggregate(msg.Content, (current, blocked) => current.Replace(blocked, string.Empty));
            var matches = _sanitizeMentions.Matches(sanitizedMsg);

            if (matches.Count > 0)
            {
                foreach (var match in matches.Select(x => x.Value))
                {
                    sanitizedMsg = sanitizedMsg.Replace(match, string.Empty);
                }
            }

            if (socketReaction.Emote.Name.Equals("☣️"))
            {
                Console.WriteLine("Toxic message marked");
                var mlMessage = new MlMessage
                {
                    Toxic = true,
                    Message = sanitizedMsg
                };
                await dbContext.MlMessages.AddAsync(mlMessage);
                await dbContext.SaveChangesAsync();
                return;
            }
            if (socketReaction.Emote.Name.Equals("🧼"))
            {
                Console.WriteLine("Clean message marked");
                var mlMessage = new MlMessage
                {
                    Toxic = false,
                    Message = msg.Content
                };
                await dbContext.MlMessages.AddAsync(mlMessage);
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task TransferToxicMessages(SocketMessage socketMessage)
        {
            if (!(socketMessage.Channel is SocketTextChannel channel)) return;
            if (channel.Id != 757841782057730079 && channel.Id != 757846331955085323) return;
            if (socketMessage.Content.StartsWith("!toxic") || socketMessage.Content.StartsWith("cs")) return;
            var guild = channel.Guild;
            if (channel.Id == 757846331955085323)
            {
                var dbContext = _dbContext;
                var mlMessage = new MlMessage
                {
                    Toxic = true,
                    Message = socketMessage.Content
                };
                await dbContext.MlMessages.AddAsync(mlMessage);
                await dbContext.SaveChangesAsync();
                return;
            }
            var toxicChannel = guild.GetTextChannel(757841495595286579);
            await toxicChannel.SendMessageAsync(socketMessage.Content);
            await socketMessage.DeleteAsync();
        }

        private async Task SubmitMessageForManualReview(SocketMessage socketMessage)
        {
            if (!(socketMessage.Channel is SocketTextChannel channel)) return;
            if (channel.Id == 757841495595286579 || channel.Id == 704726568861302814 || channel.Id == 757841782057730079) return;
            await using var fs = File.OpenRead(@"Config.json");
            var config = await JsonSerializer.DeserializeAsync<ConfigModel>(fs);
            if (config.Ignored.Any(x => x.Equals(channel.Id)))
            {
                return;
            }
            if (socketMessage.Author.IsBot) return;
            var dbContext = _dbContext;
            var msg = socketMessage.Content;
            if (msg.StartsWith("!toxic")) return;
            var matches = _sanitizeMentions.Matches(msg);

            if (matches.Count > 0)
            {
                foreach (var match in matches.Select(x => x.Value))
                {
                    msg = msg.Replace(match, string.Empty);
                }
            }

            var input = new ModelInput { Message = socketMessage.Content };
            var result = ConsumeModel.Predict(input);
            var score = Math.Round(result.Score[1] * 100, 2);
            var dbMsg = await dbContext.MlMessages.FirstOrDefaultAsync(x => x.Message.ToLower().Equals(msg.ToLower()));
            if (dbMsg is null)
            {
                if (score >= 70)
                {
                    var toxicChannel = channel.Guild.GetTextChannel(757841495595286579);
                    await toxicChannel.SendMessageAsync(msg);
                    await channel.SendMessageAsync($"{socketMessage.Author} Woah slow down silly! That looked {score}% toxic to me!");
                }
            }
        }

        private readonly Regex _sanitizeMentions = new Regex("<.*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
