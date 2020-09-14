using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using System;
using System.IO;

namespace LoggingWithSerilogInAspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");

            SinkOptions sinkOptions = new SinkOptions()
            {
                TableName = "LogTable",
                AutoCreateSqlTable = true,
            };

            ColumnOptions columnOptions = new ColumnOptions();
            columnOptions.Store.Add(StandardColumn.LogEvent);
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.TimeStamp.ConvertToUtc = true;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning() // Set the minimun log level
                .WriteTo.File(formatter: new CompactJsonFormatter(), "Logs\\log-.txt",
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7) // this is for logging into file system
                .WriteTo.MSSqlServer(connectionString: connectionString, sinkOptions: sinkOptions, columnOptions: columnOptions) // this is for logging into database
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                            .UseStartup<Startup>()
                            .ConfigureLogging(logging => { logging.ClearProviders(); }) // clearing all other logging providers
                            .UseSerilog(); // Using serilog 
        }
    }
}
