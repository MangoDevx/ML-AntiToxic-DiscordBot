using Discord.Commands;
using MLNetDBot.EFDatabase;
using SampleClassification.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MLNetDBot.Commands
{
    public class ToxicCheck : ModuleBase<SocketCommandContext>
    {
        private readonly DataContext _dbContext;

        public ToxicCheck(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("toxic")]
        [Summary("States the toxic % ofa  message")]
        public async Task Toxic([Remainder] string message)
        {
            var dbContext = _dbContext;
            var input = new ModelInput { Message = message };
            var result = ConsumeModel.Predict(input);
            var score = Math.Round(result.Score[1] * 100, 2);
            await ReplyAsync($"Toxic: {result.Prediction} Percentage: {score}%");
            if (dbContext.MlMessages.FirstOrDefault(x => x.Message.ToLower().Equals(message.ToLower())) is null)
            {
                if (score >= 90)
                {
                    var channel = Context.Guild.GetTextChannel(757841495595286579);
                    await channel.SendMessageAsync(message);
                }
            }
        }
    }
}
