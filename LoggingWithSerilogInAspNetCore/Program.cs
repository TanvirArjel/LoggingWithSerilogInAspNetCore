using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.MSSqlServer;
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

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information() // Set the minimun log level
                .WriteTo.File("Logs\\log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit:7) // this is for logging into file system
                .WriteTo.MSSqlServer(connectionString : connectionString,tableName: "LogTable",
                columnOptions: new ColumnOptions(), autoCreateSqlTable: true) // this is for logging into database
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging => { logging.ClearProviders(); }) // clearing all other logging providers
                .UseSerilog(); // Using serilog 
    }
}
