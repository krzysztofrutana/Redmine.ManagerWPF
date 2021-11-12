using Microsoft.Extensions.DependencyInjection;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Desktop.Helpers;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Linq;
using System.Reflection;

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
            services.AddDbContext<Context>();

            return services;
        }

        public static IServiceCollection RegisterMessageBoxService(this IServiceCollection services)
        {
            services.AddTransient<IMessageBoxService, MessageBoxService>();

            return services;
        }
    }
}