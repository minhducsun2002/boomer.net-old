using System.Threading.Tasks;
using Qmmands;

using Pepper.Classes;
using Pepper.Classes.Command;

namespace Pepper.Commands
{
    public class Ping : PepperCommand
    {
        [Command("ping")]
        [Category("General")]
        [Description("Pong!")]
        public async Task Exec()
        {
            await Context.Message.Channel.SendMessageAsync(
                $"Pong!\nHeartbeat roundtrip latency : {Context.Client.Latency}ms."
            );
        }
    }
}