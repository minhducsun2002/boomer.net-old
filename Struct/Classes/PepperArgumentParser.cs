using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qmmands;

namespace Pepper.Classes.Command
{
    public class PepperArgumentParser : IArgumentParser
    {
        private static readonly char Quote = '"';

        public ValueTask<ArgumentParserResult> ParseAsync(CommandContext context)
        {
            var command = context.Command;
            var rawArguments = context.RawArguments.TrimStart();
            var parameters = new Dictionary<Parameter, object>();

            // Initialize to default values.
            foreach (var param in command.Parameters)
                parameters[param] = param.Type == typeof(string) ? null : param.DefaultValue;

            // sort parameters into two types : with and without flags
            var flagParameters = new Dictionary<string, Parameter>();
            var nonFlagParameters = new LinkedList<Parameter>();
            foreach (var param in context.Command.Parameters)
            {
                var flagAttribute = param.Attributes.FirstOrDefault(attrib => attrib is FlagAttribute);
                if (flagAttribute == null)
                    nonFlagParameters.AddLast(param);
                else
                    foreach (var flag in ((FlagAttribute) flagAttribute).Flags)
                        flagParameters.Add(flag, param);
            }
            
            // sort dictionary by key.
            // this is done to prefer longer flags over shorter colliding ones (/flag1 over /f)
            flagParameters = flagParameters
                .OrderByDescending(pair => pair.Key, StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            
            var splitArguments = SmartSplit(rawArguments, Quote);
            // filter for arguments that correspond to flags
            var remainingArguments = splitArguments.Where(argument =>
            {
                string flagPrefix = default;
                // if not flag, leave it as-is
                if (!flagParameters.Any(flagMap => argument.StartsWith(flagPrefix = flagMap.Key))) return true;
                
                var parameter = flagParameters[flagPrefix];
                
                // overwrite values if-and-only-if boolean flag ones
                // this must be done at the parser level, since type readers will always get an empty string for boolean flag params.
                if (parameter.Type == typeof(bool) && parameter.Attributes.Any(attrib => attrib is FlagAttribute))
                    parameters[parameter] = bool.TrueString;
                else
                {
                    var passingArgument = argument.Substring(flagPrefix.Length);
                    parameters[parameter] = passingArgument.StartsWith(Quote) && passingArgument.EndsWith(Quote)
                        ? passingArgument[1..^1] : passingArgument;
                }
                
                // don't consider this anymore in other parameters
                return false;
            }).ToArray();

            
            foreach (var leftoverArgument in remainingArguments)
            {
                if (!nonFlagParameters.Any()) break;
                
                // take the first parameter and remove it from the queue
                var param = nonFlagParameters.First.Value;
                nonFlagParameters.RemoveFirst();

                if (param.IsRemainder)
                {
                    parameters[param] = string.Join(' ', remainingArguments);
                    break;
                }
                
                parameters[param] = leftoverArgument;
            }

            return new DefaultArgumentParserResult(command, parameters);
        }

        private static ImmutableArray<string> SmartSplit(string content, char quote, char whitespace = ' ', char escape = '\\')
        {
            var output = new List<string>();
            var piece = new StringBuilder();
            
            bool isEscaping = false, isQuoting = false;
            foreach (var character in content)
            {
                if (isEscaping)
                {
                    piece.Append(character);
                    isEscaping = false;
                    continue;
                }

                if (character == escape)
                {
                    isEscaping = true;
                    continue;
                }

                if (character == quote) isQuoting = !isQuoting;

                if (character == whitespace)
                {
                    // chunk
                    output.Add(piece.ToString());
                    piece.Clear();
                    continue;
                }

                piece.Append(character);
            }

            if (piece.Length >= 0) output.Add(piece.ToString());
            return output.ToImmutableArray();
        }
    }
}