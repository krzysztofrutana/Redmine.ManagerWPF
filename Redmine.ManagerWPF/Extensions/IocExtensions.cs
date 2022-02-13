using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Redmine.ManagerWPF.Desktop.Extensions
{
    public static class IocExtensions
    {
        public static ILogger<T> GetLoggerForType<T>(this Ioc container) where T : class
        {
            var loggerFactory = Ioc.Default.GetRequiredService<ILoggerFactory>();

            var logger = loggerFactory.CreateLogger<T>();

            return logger;
        }
    }
}
