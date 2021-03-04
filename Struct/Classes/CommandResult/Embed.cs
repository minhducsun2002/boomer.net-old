using Discord;

namespace Pepper.Classes.Command.Result
{
    public class EmbedResult : PepperCommandResult
    {
        public override bool IsSuccessful => true;
        public Embed[] Embeds;
        public Embed NoEmbed;
    }
}