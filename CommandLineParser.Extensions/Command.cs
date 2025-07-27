using System;
using System.Linq;
using System.Reflection;
using CommandLine.Text;

namespace CommandLine.Extensions
{
    /// <summary>
    /// The base class for a program command.
    /// </summary>
    public abstract class Command
    {
        private static readonly Type[] Commands = Assembly.GetEntryAssembly()
            .GetTypes()
            .Where(t => !t.IsNested && t.BaseType == typeof(Command) && t.GetCustomAttribute<VerbAttribute>(false) != null)
            .ToArray();

        private HelpText helpText;

        /// <summary>
        /// Gets the default help text for this command.
        /// </summary>
        protected HelpText HelpText
        {
            get {
                if(helpText == null)
                {
                    if(ParseResult.Tag == ParserResultType.Parsed)
                    {
                        // Build the help text for a command that was successfully parsed
                        helpText = HelpText.AutoBuild(ParseResult, text => text, text => text, true);

                        var nestedCommands = this.GetType()
                            .GetNestedCommands()
                            .ToArray();

                        if(nestedCommands.Length > 0)
                            helpText.AddVerbs(nestedCommands);
                    }
                    else
                    {
                        helpText = HelpText.AutoBuild(ParseResult);
                    }
                }

                return helpText;
            }
        }

        /// <summary>
        /// Gets the <see cref="ParserResult{T}"/> generated from parsing this command.
        /// </summary>
        protected ParserResult<object> ParseResult { get; private set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Parses the specified arguments using the default parser and executes the respective command, if one is found.
        /// </summary>
        /// <param name="args">The arguments sent to the application.</param>
        /// <returns>A <see cref="ParserResult{T}"/> containing the appropriate instance with parsed values as a <see cref="object"/> and a sequence of <see cref="Error"/>.</returns>
        public static ParserResult<object> ParseAndExecute(string[] args)
        {
            return ParseAndExecute(Parser.Default, args);
        }

        /// <summary>
        /// Parses the specified arguments and executes the respective command, if one is found.
        /// </summary>
        /// <param name="parser">The parser to use to parse the command line arguments.</param>
        /// <param name="args">The arguments sent to the application.</param>
        /// <returns>A <see cref="ParserResult{T}"/> containing the appropriate instance with parsed values as a <see cref="object"/> and a sequence of <see cref="Error"/>.</returns>
        public static ParserResult<object> ParseAndExecute(Parser parser, string[] args)
        {
            var result = parser.ParseCommands(args, Commands);

            result.WithParsed(obj => {
                if(!(obj is Command cmd))
                    throw new InvalidOperationException($"Parsed command (of type {obj.GetType().FullName}) does not inherit from {typeof(Command).FullName}");

                cmd.ParseResult = result;
                cmd.Execute();
            });

            return result;
        }
    }
}
