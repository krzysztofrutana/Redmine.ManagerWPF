using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Helpers
{
    public static class SettingsHelper
    {
        public static string GetApiKey()
        {
            return Settings.Default.Apikey;
        }

        public static void SetApiKey(string value)
        {
            Settings.Default.Apikey = value;
        }

        public static string GetUrl()
        {
            return Settings.Default.Url;
        }

        public static void SetUrl(string value)
        {
            Settings.Default.Url = value;
        }

        public static string GetDatabaseName()
        {
            return Settings.Default.DatabaseName;
        }

        public static void SetDatabaseName(string value)
        {
            Settings.Default.DatabaseName = value;
        }

        public static string GetServerName()
        {
            return Settings.Default.ServerName;
        }

        public static void SetServerName(string value)
        {
            Settings.Default.ServerName = value;
        }

        public static void Save()
        {
            Settings.Default.Save();
        }
    }
}