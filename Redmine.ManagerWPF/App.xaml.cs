using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Redmine.ManagerWPF.Database;
using Redmine.ManagerWPF.Desktop.Extensions;

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
               .RegisterLoggerFactory()
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