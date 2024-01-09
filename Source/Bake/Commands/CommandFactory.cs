// MIT License
// 
// Copyright (c) 2021-2023 Rasmus Mikkelsen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Bake.Core;
using Bake.Extensions;
using Bake.Services;
using Bake.ValueObjects;
using Bake.ValueObjects.Destinations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bake.Commands
{
    public partial class CommandFactory : ICommandFactory
    {
        private const string MethodName = "ExecuteAsync";

        [GeneratedRegex("(?<char>[A-Z])")]
        private static partial Regex UpperReplacer();

        private readonly ILogger<CommandFactory> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDestinationParser _destinationParser;
        private readonly IPlatformParser _platformParser;

        public CommandFactory(
            ILogger<CommandFactory> logger,
            IServiceProvider serviceProvider,
            IDestinationParser destinationParser,
            IPlatformParser platformParser)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _destinationParser = destinationParser;
            _platformParser = platformParser;
        }

        public CommandLineApplication Create(IEnumerable<Type> types)
        {
            var app = new CommandLineApplication();
            app.ValueParsers.Add(ValueParser.Create(
                typeof(SemVer),
                (argName, value, _) =>
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return SemVer.With(1);
                    }

                    if (!SemVer.TryParse(value, out var version))
                    {
                        throw new FormatException($"'{value}' is an invalid SemVer value for argument '{argName}'");
                    }

                    return version;
                }));
            app.ValueParsers.Add(ValueParser.Create(
                typeof(Destination),
                (argName, value, _) =>
                {
                    if (value == null)
                    {
                        return null!;
                    }

                    if (!_destinationParser.TryParse(value, out var destination))
                    {
                        throw new FormatException($"'{value}' is an invalid Destination value for argument '{argName}'");
                    }

                    return destination;
                }));
            app.ValueParsers.Add(ValueParser.Create(
                typeof(Destination[]),
                (argName, value, _) =>
                {
                    return value?.Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v =>
                        {
                            if (!_destinationParser.TryParse(v, out var destination))
                            {
                                throw new FormatException(
                                    $"'{value}' is an invalid Destination value for argument '{argName}'");
                            }

                            return destination;
                        })
                        .ToArray() ?? Array.Empty<Destination>();
                }));
            app.ValueParsers.Add(ValueParser.Create(
                typeof(Platform),
                (argName, value, _) =>
                {
                    if (value == null)
                    {
                        return null!;
                    }

                    if (!_platformParser.TryParse(value, out var platform))
                    {
                        throw new FormatException($"'{value}' is an invalid Platform value for argument '{argName}'");
                    }

                    return platform;
                }));
            app.ValueParsers.Add(ValueParser.Create(
                typeof(Platform[]),
                (argName, value, _) =>
                {
                    return value?
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v =>
                        {
                            if (!_platformParser.TryParse(v, out var platform))
                            {
                                throw new FormatException(
                                    $"'{value}' is an invalid Platform value for argument '{argName}'");
                            }

                            return platform;
                        })
                        .ToArray() ?? Array.Empty<Platform>();
                }));


            app.Description =
@"Bake is a convention based build tool that focuses on minimal to none
effort to configure and setup. Ideally you should be able to run bake in
any repository with minimal arguments and get the ""expected"" output or
better. This however comes at the cost of conventions and how well Bake
works on a project all depends on how many of the conventions that
project follows.

https://github.com/rasmus/Bake";
            app.OnExecute(() =>
            {
                Console.WriteLine(app.GetHelpText());
                return ExitCodes.Core.NoCommand;
            });

            app.ShowHint();
            app.HelpOption(true);
            app.VersionOption(
                "-v|--version",
                GetVersion,
                GetVersion);

            var commandType = typeof(ICommand);
            var cancellationTokenType = typeof(CancellationToken);

            foreach (var type in types)
            {
                if (!commandType.IsAssignableFrom(type))
                {
                    throw new ArgumentException(
                        $"Type '{type.PrettyPrint()}' is not of type '{commandType.PrettyPrint()}'");
                }

                var commandAttribute = type.GetCustomAttribute<CommandAttribute>();
                if (commandAttribute == null)
                {
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' does not have a command attribute");
                }

                var methodInfos = type
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(mi => string.Equals(mi.Name, MethodName, StringComparison.Ordinal))
                    .ToList();

                if (methodInfos.Count != 1)
                {
                    throw new ArgumentException(
                        $"Type '{type.PrettyPrint()}' does not have exactly one '{MethodName}' method");
                }

                var methodInfo = methodInfos.Single();

                var command = app.Command(commandAttribute.Name, cmd =>
                {
                    var options = new List<(CommandOption?, Type)>();
                    foreach (var parameterInfo in methodInfo.GetParameters())
                    {
                        if (parameterInfo.ParameterType == cancellationTokenType)
                        {
                            options.Add((null, cancellationTokenType));
                            continue;
                        }

                        var argumentAttribute = parameterInfo.GetCustomAttribute<ArgumentAttribute>();
                        var argumentName = UpperReplacer().Replace(parameterInfo.Name!, m => $"-{m.Groups["char"].Value.ToLowerInvariant()}");

                        string? defaultValue = null;
                        if (parameterInfo.HasDefaultValue)
                        {
                            if (parameterInfo.DefaultValue != null)
                            {
                                defaultValue = parameterInfo.DefaultValue?.ToString();
                            }
                            else if (parameterInfo.ParameterType == typeof(bool))
                            {
                                defaultValue = "false";
                            }
                        }

                        var option = cmd.Option(
                            $"--{argumentName} <{parameterInfo.Name!.ToUpperInvariant()}>",
                            argumentAttribute?.Description ?? string.Empty,
                            parameterInfo.HasDefaultValue
                                ? CommandOptionType.SingleOrNoValue
                                : CommandOptionType.SingleValue);
                        option.DefaultValue = defaultValue;
                        
                        options.Add((option, parameterInfo.ParameterType));
                    }

                    cmd.OnExecuteAsync(async c =>
                    {
                        var command = _serviceProvider.GetRequiredService(type);

                        var values = options
                            .Select(t => t.Item2 == cancellationTokenType
                                ? c
                                : Parse(t.Item1!, GetParser(app.ValueParsers, t.Item2)))
                            .ToArray();


                        try
                        {
                            return await (Task<int>) methodInfo.Invoke(command, values)!;
                        }
                        catch (Exception e)
                        {
                            _logger.LogCritical(
                                e,
                                "Unexpected exception while executing command {CommandType}",
                                commandType.PrettyPrint());
                            return ExitCodes.Core.UnexpectedError;
                        }
                    });
                });

                command.Description = commandAttribute.Description;
            }

            return app;
        }

        private static string GetVersion()
        {
            return typeof(CommandFactory).Assembly.GetName().Version!.ToString();
        }

        private static IValueParser GetParser(
            ValueParserProvider valueParserProvider,
            Type type)
        {
            var valueParser = valueParserProvider.GetParser(type);
            if (valueParser == null)
            {
                throw new ArgumentException(
                    $"Do not know how to parse {type.PrettyPrint()}");
            }

            return valueParser;
        }

        private static object? Parse(
            CommandOption option,
            IValueParser valueParser)
        {
            var stringValue = (option.Values.FirstOrDefault() ?? string.Empty).Trim('"');
            return valueParser.Parse(
                option.ShortName ?? option.LongName,
                stringValue,
                CultureInfo.InvariantCulture);
        }
    }
}
