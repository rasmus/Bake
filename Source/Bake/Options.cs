using CommandLine;
using Serilog.Events;

namespace Bake
{
    public class CommandLineOptions
    {
        [Option(
            'l',
            "log-level",
            Required = false,
            Default = LogEventLevel.Information,
            HelpText = "Defines the log level")]
        public LogEventLevel LogLevel { get; set; }
    }
}
