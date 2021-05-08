using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bake.Extensions;
using McMaster.Extensions.CommandLineUtils;
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
                    foreach (var parameterInfo in methodInfo.GetParameters())
                    {
                        if (parameterInfo.ParameterType == cancellationTokenType)
                        {
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
                    }

                    cmd.OnExecuteAsync(c =>
                    {
                        return Task.FromResult(0);
                    });
                });


            }

            return app;
        }
    }
}
