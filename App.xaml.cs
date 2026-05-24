using System;
using System.IO;
using System.Windows;
using Serilog;

namespace HorizonPulse
{
    public partial class App : Application
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "horizonpulse-.log");

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Initialize Serilog with console and file sinks
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(LogFilePath, 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Horizon Pulse application starting");
            Log.Information("Log file path: {LogPath}", LogFilePath);
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Log.Information("Horizon Pulse application exiting");
            Log.CloseAndFlush();
        }
    }
}
