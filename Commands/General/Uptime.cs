using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;

using Pepper.Classes;
using Pepper.Classes.Command;
using Pepper.Utilities;

namespace Pepper.Commands
{
    public class Uptime : PepperCommand
    {
        [Command("uptime")]
        [Category("General")]
        [Description("How long have I been up?")]
        public async Task Exec()
        {
            var interval = (DateTime.Now - Process.GetCurrentProcess().StartTime);
            // construct friendly time
            var timeComponents = new[]
                {
                    (interval.Days, "day"),
                    (interval.Hours, "hour"),
                    (interval.Minutes, "minute"),
                    (interval.Seconds, "second")
                }
                .Select(field =>
                    field.Item1 >= 1 ? $"**{field.Item1}** {field.Item2 + StringUtilities.Plural(field.Item1)}" : "")
                .Where(output => output.Length > 0)
                .ToArray();

            var timeString = timeComponents.Length == 1
                ? timeComponents[0]
                : string.Join(',', new ArraySegment<string>(timeComponents, 0, timeComponents.Length - 1))
                  + $" and {timeComponents[^1]}";
                
            await Context.Channel.SendMessageAsync(
                $"I've been running for {timeString}. "
                + (interval > new TimeSpan(0, 30, 0) ? "Quite a while, heh? " : "")
            );
        }
    }
}