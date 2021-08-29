using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Redmine.ManagerWPF.Desktop.Extensions;
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
               .BuildServiceProvider());
        }
    }
}