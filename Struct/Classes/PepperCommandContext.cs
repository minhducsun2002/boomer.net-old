using System;
using Discord.WebSocket;
using Pepper.Services.Main;

namespace Pepper.Classes.Command
{
    public class PepperCommandContext : Qmmands.CommandContext
    {
        public PepperCommandContext(IServiceProvider services) : base(services) {}
        public SocketUser Author;
        public DiscordSocketClient Client;
        public ISocketMessageChannel Channel;
        public SocketUserMessage Message;
        public CommandService CommandService;
        public string Prefix;
        public new string Alias;
    }
}