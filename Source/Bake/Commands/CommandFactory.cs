using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bake.Core;
using Bake.Extensions;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bake.Commands
{
    public class CommandFactory : ICommandFactory
    {
        private const string MethodName = "ExecuteAsync";
        private static readonly Regex UpperReplacer = new Regex(
            "(?<char>[A-Z])", RegexOptions.Compiled);

        private readonly ILogger<CommandFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CommandFactory(
            ILogger<CommandFactory> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public CommandLineApplication Create(IEnumerable<Type> types)
        {
            var app = new CommandLineApplication();
            app.ValueParsers.Add(ValueParser.Create(
                typeof(SemVer),
                (argName, value, _) =>
                {
                    if (value == null)
                    {
                        return null;
                    }

                    if (!SemVer.TryParse(value, out var version))
                    {
                        throw new FormatException($"Invalid SemVer value for {argName}");
                    }

                    return version;
                }));

            app.HelpOption(true);

            var commandType = typeof(ICommand);
            var cancellationTokenType = typeof(CancellationToken);

            foreach (var type in types)
            {
                if (!commandType.IsAssignableFrom(type))
                {
                    throw new ArgumentException(
                        $"Type '{type.PrettyPrint()}' is not of type '{commandType.PrettyPrint()}'");
                }

                var verb = type.GetCustomAttribute<CommandVerbAttribute>()?.Name;
                if (string.IsNullOrEmpty(verb))
                {
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' does not have a verb");
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

                app.Command(verb, cmd =>
                {
                    var options = new List<(CommandOption, Type)>();
                    foreach (var parameterInfo in methodInfo.GetParameters())
                    {
                        if (parameterInfo.ParameterType == cancellationTokenType)
                        {
                            options.Add((null, cancellationTokenType));
                            continue;
                        }

                        var argumentAttribute = parameterInfo.GetCustomAttribute<ArgumentAttribute>();
                        var argumentName = UpperReplacer.Replace(parameterInfo.Name, m => $"-{m.Groups["char"].Value.ToLowerInvariant()}");

                        string defaultValue = null;
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
                            $"--{argumentName} <{parameterInfo.Name.ToUpperInvariant()}>",
                            argumentAttribute?.Description ?? string.Empty,
                            parameterInfo.HasDefaultValue
                                ? CommandOptionType.SingleOrNoValue
                                : CommandOptionType.SingleValue);
                        option.DefaultValue = defaultValue;
                        
                        options.Add((option, parameterInfo.ParameterType));
                    }

                    cmd.OnExecuteAsync(c =>
                    {
                        var command = _serviceProvider.GetRequiredService(type);

                        var values = options
                            .Select(t => t.Item2 == cancellationTokenType
                                ? c
                                : Parse(t.Item1, app.ValueParsers.GetParser(t.Item2)))
                            .ToArray();

                        return (Task<int>) methodInfo.Invoke(
                            command,
                            values);
                    });
                });
            }

            return app;
        }

        private static object Parse(
            CommandOption option,
            IValueParser valueParser)
        {
            var stringValue = option.Values.FirstOrDefault() ?? string.Empty;
            return valueParser.Parse(
                option.ShortName ?? option.LongName,
                stringValue,
                CultureInfo.InvariantCulture);
        }
    }
}
