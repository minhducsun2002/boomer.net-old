using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Qmmands;

using Pepper.Classes;
using Pepper.Classes.Command;

namespace Pepper.Commands
{
    public class Invite : PepperCommand
    {
        /*
         * cache the application,
         * so that we don't have to fetch it again every time someone wants an invite
         */
        private RestApplication _application;
        
        [Command("invite")]
        [Category("General")]
        [Description("Generate a link to invite me to your place.")]
        public async Task Exec()
        {
            _application ??= await Context.Client.GetApplicationInfoAsync();
            
            // generate permission fields
            var permissions = new[]
            {
                GuildPermission.Administrator,
                GuildPermission.ManageMessages, GuildPermission.UseExternalEmojis,
                GuildPermission.ViewChannel,GuildPermission.SendMessages, GuildPermission.EmbedLinks, GuildPermission.AttachFiles,
                GuildPermission.Speak, GuildPermission.AddReactions, GuildPermission.ChangeNickname
            }.Aggregate((output, perm) => output | perm);
            
            var invite =
                $"https://discord.com/oauth2/authorize?client_id={_application.Id}&scope=bot&permissions={(ulong) permissions}";
            
            await Context.Message.Channel.SendMessageAsync(
                    "",
                    false,
                    new EmbedBuilder
                    {
                        Description = $"You may invite me to your server using [this link]({invite})."
                    }.Build()
            );
        }
    }
}