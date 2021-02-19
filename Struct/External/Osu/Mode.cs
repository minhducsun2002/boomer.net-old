using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Qmmands;

namespace Pepper.External.Osu
{
    public enum GameMode
    {
        [GameModeString("osu", FriendlyName = "osu!")]
        Osu,
        [GameModeString("taiko", FriendlyName = "osu!taiko")]
        Taiko,
        [GameModeString("fruits", FriendlyName = "osu!catch")]
        Catch,
        [GameModeString("mania", FriendlyName = "osu!mania")]
        Mania
    }

    internal class GameModeStringAttribute : Attribute
    {
        public string UrlPath { get; }
        public string FriendlyName { get; set; }
        public GameModeStringAttribute(string value)
        {
            UrlPath = value;
        }
    }

    public static class GameModeString
    {
        private static GameModeStringAttribute getAttribute(GameMode mode)
        {
            var type = mode.GetType();
            return (GameModeStringAttribute) type.GetField(mode.ToString())
                .GetCustomAttribute(typeof(GameModeStringAttribute), false);
        }
        
        public static string ToFriendlyName(this GameMode mode)
        {
            return getAttribute(mode).FriendlyName;
        }
        
        public static string ToUrlPath(this GameMode mode)
        {
            return getAttribute(mode).UrlPath;
        }
    }

    public class GameModeTypeParser : TypeParser<GameMode>
    {
        public static readonly GameModeTypeParser Instance = new GameModeTypeParser();
        public override ValueTask<TypeParserResult<GameMode>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var defaultGameMode = GameMode.Osu;
            try
            {
                defaultGameMode = Enum.GetValues(typeof(GameMode)).Cast<GameMode>()
                    .First(mode => mode.ToUrlPath() == value);
            }
            catch { /* ignored */ }

            return new ValueTask<TypeParserResult<GameMode>>(TypeParserResult<GameMode>.Successful(defaultGameMode));
        }
    }
}