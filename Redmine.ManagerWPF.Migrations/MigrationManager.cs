using CommunityToolkit.Mvvm.DependencyInjection;
using FluentMigrator.Runner;
using Redmine.ManagerWPF.Database.Helpers;
using Redmine.ManagerWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redmine.ManagerWPF.Database
{
    public class MigrationManager
    {
        public async void MigrateDatabase()
        {
            var databaseService = Ioc.Default.GetRequiredService<DatabaseManager>();


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
                catch
                {
                }
            }
        }
    }
}
