namespace Dashing.CommandLine {
    using System;

    using McMaster.Extensions.CommandLineUtils;

    using Serilog;
    using Serilog.Events;

    public static class LogConfiguration {
        public static void ConfigureLogging(this CommandLineApplication app) {
            var logLevel = app.Option("-l|--log <level>", "The logging level, one of verbose,debug,information,warning,error,fatal. Defaults to warning", CommandOptionType.SingleValue);
            if (!logLevel.HasValue() || !Enum.TryParse(logLevel.Value(), true, out LogEventLevel logEventLevel)) {
                logEventLevel = LogEventLevel.Warning;
            }

            Log.Logger = new LoggerConfiguration().MinimumLevel.Is(logEventLevel)
                                                  .WriteTo.Console()
                                                  .CreateLogger();
        }
    }
}