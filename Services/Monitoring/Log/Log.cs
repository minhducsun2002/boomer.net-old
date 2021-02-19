using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Pastel;

namespace Pepper.Services.Monitoring
{
    namespace Log
    {
            public enum LogType
        {
            None,
            Success,
            Info,
            Warning,
            Error
        }

        public class LogTag
        {
            public string Name { get; set; }
            public string ForegroundColor = "#ffffff";
            public string BackgroundColor = "#000000";
        }
        
        public class LogEntry
        {
            /// <summary>
            /// A list of tags for this log.
            /// </summary>
            public LogTag[] Tags = new LogTag[] { };

            /// <summary>
            /// Log content.
            /// </summary>
            public string Content = "";
        }
    }
    
    public class LogService
    {
        public void Write(Log.LogType type, Log.LogEntry entry)
        {
            Color fg = Color.Black, bg = Color.White;
            string prefix = "";
            switch (type)
            {
                case Log.LogType.Error:
                    bg = Color.Red;
                    prefix = "[✕]"; break;
                case Log.LogType.Info:
                    bg = Color.Blue;
                    prefix = "[i]"; break;
                case Log.LogType.Success:
                    bg = Color.LightGreen;
                    fg = Color.Black;
                    prefix = "[✓]"; break;
                case Log.LogType.Warning:
                    bg = Color.Yellow;
                    prefix = "[!]"; break;
            }

            var output = (entry.Content ?? "").Split("\n");
            var currentTime = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
            var tags = string.Join(" ", entry.Tags.Select(tag => $"[{tag.Name}]".Pastel(tag.ForegroundColor).PastelBg(tag.BackgroundColor)));
            var tagLength = string.Join(" ", entry.Tags.Select(tag => $"[{tag.Name}]")).Length;
            if (tagLength > 0) tags += " ";
            Console.WriteLine($"{currentTime.Pastel("#ff00ff")}| {prefix.Pastel(fg).PastelBg(bg)} {tags}{output[0]}");
            foreach (var segment in new ArraySegment<string>(output, 1, output.Length - 1))
                Console.WriteLine($"{new string(' ', currentTime.Length + 2 + prefix.Length + 1 + tagLength)} {segment}");
        }
    }
}