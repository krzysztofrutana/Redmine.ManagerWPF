using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Database.Helpers;
using Redmine.ManagerWPF.Helpers;

namespace Redmine.ManagerWPF.Database
{
    public class MigrationManager
    {
        public async void MigrateDatabase()
        {
            var databaseService = Ioc.Default.GetRequiredService<DatabaseManager>();
            var loggerFactory = Ioc.Default.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<MigrationManager>();

            var connectionString = "";
            var databaseName = SettingsHelper.GetDatabaseName();
            var server = SettingsHelper.GetServerName();

            if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(server))
            {
                connectionString = $"Server={server};Database={databaseName};Trusted_Connection=True;";
            }

            if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(server))
            {
                try
                {
                    await databaseService.CreateDatabaseAsync(databaseName);

                    var fluentServiceProvider = ServiceProviderHelper.CreateServiceProviderForFluentMigrator(connectionString);

                    var migrationService = fluentServiceProvider.GetService(typeof(IMigrationRunner)) as IMigrationRunner;

                    migrationService.ListMigrations();
                    migrationService.MigrateUp();
                }
                catch (Exception ex)
                {
                    logger.LogError("{0} {1}", nameof(MigrateDatabase), ex.Message);
                }
            }
        }
    }
}
