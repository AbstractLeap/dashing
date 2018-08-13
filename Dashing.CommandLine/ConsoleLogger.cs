namespace Dashing.CommandLine {
    using System;

    using Microsoft.Extensions.CommandLineUtils;

    using Serilog;
    using Serilog.Events;

    public static class LogConfiguration {
        public static void Configure(CommandOption option) {
            if (!option.HasValue() || !Enum.TryParse(option.Value(), true, out LogEventLevel logEventLevel)) {
                logEventLevel = LogEventLevel.Warning;
            }

            Log.Logger = new LoggerConfiguration().MinimumLevel.Is(logEventLevel)
                                                  .WriteTo.Console()
                                                  .CreateLogger();
        }
    }
}