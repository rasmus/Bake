using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Bake.Extensions;
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

        public RootCommand Create(IEnumerable<Type> types)
        {
            var commandType = typeof(ICommand);
            var cancellationTokenType = typeof(CancellationToken);
            var rootCommand = new RootCommand
                {
                    TreatUnmatchedTokensAsErrors = true,
                };

            foreach (var type in types)
            {
                if (!commandType.IsAssignableFrom(type))
                {
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' is not of type '{commandType.PrettyPrint()}'");
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
                    throw new ArgumentException($"Type '{type.PrettyPrint()}' does not have exactly one '{MethodName}' method");
                }

                var methodInfo = methodInfos.Single();

                var command = new Command(verb);
                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    if (parameterInfo.ParameterType == cancellationTokenType)
                    {
                        continue;
                    }

                    var argumentAttribute = parameterInfo.GetCustomAttribute<ArgumentAttribute>();

                    var argumentName = UpperReplacer.Replace(parameterInfo.Name, m => $"-{m.Groups["char"].Value.ToLowerInvariant()}");
                    var argument = new Argument
                        {
                            ArgumentType = parameterInfo.ParameterType,
                            Arity = ArgumentArity.ExactlyOne,
                            Name = parameterInfo.Name,
                        };

                    if (parameterInfo.HasDefaultValue)
                    {
                        if (parameterInfo.DefaultValue != null)
                        {
                            argument.SetDefaultValue(parameterInfo.DefaultValue);
                        }
                        else if (parameterInfo.ParameterType == typeof(bool))
                        {
                            argument.SetDefaultValueFactory(() => false);
                        }
                    }

                    var option = new Option($"--{argumentName}")
                        {
                            Argument = argument,
                            Description = argumentAttribute?.Description,
                            IsRequired = !parameterInfo.HasDefaultValue,
                        };

                    command.AddOption(option);
                }

                _logger.LogTrace(
                    "Adding command {} with arguments {Arguments}",
                    verb,
                    command.Options.Select(o => $"{o.Name} (isRequired:{o.IsRequired})"));

                command.Handler = CommandHandler.Create(methodInfo, _serviceProvider.GetRequiredService(type));

                rootCommand.AddCommand(command);
            }

            return rootCommand;
        }
    }
}
