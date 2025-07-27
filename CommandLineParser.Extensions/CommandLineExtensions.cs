using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CommandLine.Extensions
{
    /// <summary>
    /// Static class containing extension methods for managing the command line.
    /// </summary>
    public static class CommandLineExtensions
    {
        /// <summary>
        /// Searches for nested types, that derive from <see cref="Command"/> and have the <see cref="VerbAttribute"/> applied, in the current <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type to search in.</param>
        /// <returns>The collection of nested <see cref="Command"/> types.</returns>
        internal static IEnumerable<Type> GetNestedCommands(this Type type)
        {
            if(type == null)
                throw new ArgumentNullException(nameof(type));

            return type
                .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(t => typeof(Command).IsAssignableFrom(t) && t.GetCustomAttribute<VerbAttribute>(false) != null);
        }

        /// <summary>
        /// Parses a string array of command line arguments for verb commands scenario. This differs from ParseArguments in that this has support for nested commands.
        /// The <see cref="VerbAttribute"/> must be applied to types in the array and each type must inherit from <see cref="Command"/>.
        /// </summary>
        /// <param name="parser">The parser to use.</param>
        /// <param name="args">A <see cref="string"/> array of command line arguments, normally supplied by application entry point.</param>
        /// <param name="types">A <see cref="Type"/> array used to supply command alternatives. Should contain only the top-level commands. Nested commands are found automatically.</param>
        /// <returns>A <see cref="ParserResult{T}"/> containing the appropriate instance with parsed values as a <see cref="object"/>
        /// and a sequence of <see cref="Error"/>.</returns>
        /// <returns>A <see cref="ParserResult{T}"/> containing the appropriate instance with parsed values as a <see cref="object"/> and a sequence of <see cref="Error"/>.</returns>
        /// <remarks>All types must expose a parameterless constructor and inherit from <see cref="Command"/>.</remarks>
        public static ParserResult<object> ParseCommands(this Parser parser, string[] args, params Type[] types)
        {
            if(parser == null)
                throw new ArgumentNullException(nameof(parser));

            if(args == null)
                throw new ArgumentNullException(nameof(args));

            if(types == null)
                throw new ArgumentNullException(nameof(types));

            var verb = args.FirstOrDefault();

            // If the argument starts with "-" it is not a verb, but possibly a flag
            if(verb?.StartsWith("-") ?? false)
                return parser.ParseArguments(args, types);

            var stringComparison = parser.Settings.CaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            foreach(var type in types)
            {
                if(!typeof(Command).IsAssignableFrom(type))
                    continue;

                var verbAttribute = type.GetCustomAttribute<VerbAttribute>(false);

                if(verbAttribute == null || !verbAttribute.Name.Equals(verb, stringComparison))
                    continue;

                var nested = type
                    .GetNestedCommands()
                    .ToArray();

                if(nested.Length <= 0 || args.Length <= 1)
                    break;

                return ParseCommands(parser, args.Skip(1).ToArray(), nested);
            }

            return parser.ParseArguments(args, types);
        }
    }
}
