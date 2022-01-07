using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Redmine.ManagerWPF.Database;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Redmine.ManagerWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Ioc.Default.ConfigureServices(
               new ServiceCollection()
               .RegisterDbContext()
               .RegisterAutomapper()
               .RegisterDataServices()
               .RegisterMessageBoxService()
               .BuildServiceProvider());

            MigrateDatabase();
        }

        private void MigrateDatabase()
        {
            var migrationManager = new MigrationManager();
            migrationManager.MigrateDatabase();
        }
    }
}