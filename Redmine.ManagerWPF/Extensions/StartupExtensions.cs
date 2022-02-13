using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data.Dapper;
using Redmine.ManagerWPF.Database;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Helpers.Interfaces;
using Serilog;

namespace Redmine.ManagerWPF.Desktop.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection RegisterDataServices(this IServiceCollection services)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(p => p.GetName().Name == "Redmine.ManagerWPF.Desktop" || p.GetName().Name == "Redmine.ManagerWPF.Integration")
                .ToList();

            if (assembly.Count > 0)
            {
                foreach (var item in assembly)
                {
                    item.GetTypes()
                    .Where(p => p.IsClass && typeof(IService).IsAssignableFrom(p))
                    .ToList()
                    .ForEach(p =>
                    {
                        services.AddTransient(p);
                    });
                }
            }

            return services;
        }

        public static IServiceCollection RegisterAutomapper(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }

        public static IServiceCollection RegisterDbContext(this IServiceCollection services)
        {
            services.AddSingleton<IContext, Context>();
            services.AddSingleton<DatabaseManager>();

            return services;
        }

        public static IServiceCollection RegisterMessageBoxService(this IServiceCollection services)
        {
            services.AddTransient<IMessageBoxService, MessageBoxService>();

            return services;
        }

        public static IServiceCollection RegisterFluentMigrator(this IServiceCollection services)
        {
            services.AddLogging(c => c.AddFluentMigratorConsole())
                .AddFluentMigratorCore()
                .ConfigureRunner(c => c.AddSqlServer2012()
                    .WithGlobalConnectionString("Server=.;Database=redmine_manager;Trusted_Connection=True;")
                    .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());

            return services;
        }

        public static IServiceCollection RegisterLoggerFactory(this IServiceCollection services)
        {
            string logsDirectory = Path.Combine(Environment.CurrentDirectory, "logs");
            var outputTemplate =
                    @"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}";

            var serilog = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                    .WriteTo.RollingFile(Path.Combine(logsDirectory, "log-{Date}.txt"), Serilog.Events.LogEventLevel.Debug, outputTemplate)
                    .CreateLogger();

            ILoggerFactory logger = LoggerFactory.Create(logging =>
            {
                logging.AddSerilog(serilog);

            });

            services.AddSingleton<ILoggerFactory>(logger);

            return services;
        }
    }
}