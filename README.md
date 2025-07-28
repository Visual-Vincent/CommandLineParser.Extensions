# CommandLineParser.Extensions

Unofficial, handy extensions for the excellent CommandLineParser library.

## Features

- Dedicated base class for defining application commands
- Support for nested commands

## Examples

### A simple command

```sh
myapp hello -n John
```

```csharp
using System;
using CommandLine;
using CommandLine.Extensions;

namespace MyApp;

internal class Program
{
    public static void Main(string[] args)
    {
        Command.ParseAndExecute(args);
    }
}

[Verb("hello", HelpText = "Gives you a personal hello")]
public class SayHello : Command
{
    [Option('n', "name", HelpText = "The name to say hello to", Required = true)]
    public string Name { get; set; }

    public override void Execute()
    {
        Console.WriteLine($"Hello {Name}!");
    }
}
```

### Nested commands

```sh
myapp remote add 127.0.0.1
myapp remote remove 127.0.0.1
```

```csharp
using System;
using CommandLine;
using CommandLine.Extensions;

namespace MyApp;

internal class Program
{
    public static void Main(string[] args)
    {
        Command.ParseAndExecute(args);
    }
}

[Verb("remote", HelpText = "Handles remote servers")]
public class Remote : Command
{
    [Verb("add", HelpText = "Adds a remote server")]
    public class Add : Command
    {
        [Value(0, HelpText = "The URL to the server", Required = true)]
        public string Url { get; set; }

        public override void Execute()
        {
            // TODO: Add server
            Console.WriteLine($"Server {Url} added successfully");
        }
    }

    [Verb("remove", HelpText = "Removes a remote server")]
    public class Remove : Command
    {
        [Value(0, HelpText = "The URL to the server", Required = true)]
        public string Url { get; set; }

        public override void Execute()
        {
            // TODO: Remove server
            Console.WriteLine($"Server {Url} removed successfully");
        }
    }

    public override void Execute()
    {
        // In this case solely executing the base "remote" command shouldn't do anything.
        // We can use the Command.HelpText property to print the help text of this command to the console,
        // indicating to the user that they're missing a subverb.
        Console.WriteLine(HelpText);
    }
}
```

### Making the verbs case-insensitive

```sh
myapp hELLo -n John
```

```csharp
using System;
using CommandLine;
using CommandLine.Extensions;

namespace MyApp;

internal class Program
{
    public static void Main(string[] args)
    {
        var parser = new Parser(s => {
            s.HelpWriter = Console.Error;
            s.CaseSensitive = false;
        });

        Command.ParseAndExecute(parser, args);
    }
}

[Verb("hello", HelpText = "Gives you a personal hello")]
public class SayHello : Command
{
    [Option('n', "name", HelpText = "The name to say hello to", Required = true)]
    public string Name { get; set; }

    public override void Execute()
    {
        Console.WriteLine($"Hello {Name}!");
    }
}
```
