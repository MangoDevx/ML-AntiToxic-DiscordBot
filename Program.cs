using MLNetDBot.Services;
using System;
using System.Threading.Tasks;

namespace MLNetDBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region ASCII
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(@"  __  __ _     _   _      _     ____        _   
 |  \/  | |   | \ | | ___| |_  | __ )  ___ | |_ 
 | |\/| | |   |  \| |/ _ \ __| |  _ \ / _ \| __|
 | |  | | |___| |\  |  __/ |_  | |_) | (_) | |_ 
 |_|  |_|_____|_| \_|\___|\__| |____/ \___/ \__|
                                                ");
            Console.ResetColor();
            #endregion
            await new BotInitializationService().StartInitializationAsync();
            await Task.Delay(-1);
        }
    }
}
