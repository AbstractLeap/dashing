namespace Dashing.CommandLine {
    using System;
    using System.Linq;

    using McMaster.Extensions.CommandLineUtils;

    using Serilog;
    using Serilog.Events;

    public static class LogConfiguration {
        public static void AddLogLevelOption(this CommandLineApplication app) {
            app.Option("-l|--log <level>", "The logging level, one of verbose,debug,information,warning,error,fatal. Defaults to warning", CommandOptionType.SingleValue, true);
        }

        public static void EnableLogging(this CommandLineApplication app) {
            var logLevel = app.GetOptions().SingleOrDefault(o => o.LongName == "log");
            if (logLevel == null) {
                throw new InvalidOperationException();
            }

            if (!logLevel.HasValue() || !Enum.TryParse(logLevel.Value(), true, out LogEventLevel logEventLevel)) {
                logEventLevel = LogEventLevel.Warning;
            }

            Log.Logger = new LoggerConfiguration().MinimumLevel.Is(logEventLevel)
                                                  .WriteTo.Console()
                                                  .CreateLogger();
        }
    }
}