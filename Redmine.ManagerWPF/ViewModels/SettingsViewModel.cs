using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Dapper;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Data.Dapper;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Database;
using Redmine.ManagerWPF.Desktop.Models.Settings;
using Redmine.ManagerWPF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private SettingsModel _currentSettings;

        public SettingsModel CurrentSettings
        {
            get => _currentSettings;
            set
            {
                SetProperty(ref _currentSettings, value);
            }
        }

        private string _connectionStatusText;

        public string ConnectionStatusText
        {
            get
            {
                return _connectionStatusText;
            }

            set
            {
                SetProperty(ref _connectionStatusText, value);
            }
        }

        private bool? _connected;

        public bool? Connected
        {
            get
            {
                return _connected;
            }

            set
            {
                SetProperty(ref _connected, value);
            }
        }

        public IRelayCommand SaveSettingsCommand { get; }
        public IRelayCommand ConnectionTestCommand { get; }
        public IRelayCommand CreateDatabaseCommand { get; }

        private readonly IContext _context;
        private readonly DatabaseManager _databaseManager;

        public SettingsViewModel()
        {
            _context = Ioc.Default.GetRequiredService<IContext>();
            _databaseManager = Ioc.Default.GetRequiredService<DatabaseManager>();

            CurrentSettings = new SettingsModel();
            LoadCurrentSettings();
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            ConnectionTestCommand = new RelayCommand(ConnectionTest);
            CreateDatabaseCommand = new RelayCommand(CreateDatabase);
        }

        private async void ConnectionTest()
        {
            SaveSettings();
            try
            {
                var exist = await _databaseManager.CheckDatabaseExistAsync(CurrentSettings.DatabaseName);
                if (exist)
                {
                    var migrationManager = new MigrationManager();
                    migrationManager.MigrateDatabase();

                    ConnectionStatusText = "Połączono";
                    Connected = true;
                }
                else
                {
                    ConnectionStatusText = "Podana baza nie istnieje";
                    Connected = false;
                }
            }
            catch
            {
                ConnectionStatusText = "Błąd, spróbuj ponownie";
                Connected = false;
            }

        }

        private void CreateDatabase()
        {
            SaveSettings();
            try
            {
                var migrationManager = new MigrationManager();
                migrationManager.MigrateDatabase();
                ConnectionStatusText = "Połączono";
                Connected = true;
            }
            catch
            {
                ConnectionStatusText = "Błąd, tworzenia bazy";
                Connected = false;
            }
        }

        private void LoadCurrentSettings()
        {
            CurrentSettings.ApiKey = SettingsHelper.GetApiKey();
            CurrentSettings.Url = SettingsHelper.GetUrl();
            CurrentSettings.DatabaseName = SettingsHelper.GetDatabaseName();
            CurrentSettings.ServerName = SettingsHelper.GetServerName();
        }

        private void SaveSettings()
        {
            var settings = CurrentSettings.GetType().GetProperties();

            foreach (var item in settings)
            {
                if (item.Name == nameof(CurrentSettings.ApiKey))
                {
                    SettingsHelper.SetApiKey(CurrentSettings.ApiKey);
                }
                if (item.Name == nameof(CurrentSettings.Url))
                {
                    SettingsHelper.SetUrl(CurrentSettings.Url);
                }
                if (item.Name == nameof(CurrentSettings.DatabaseName))
                {
                    SettingsHelper.SetDatabaseName(CurrentSettings.DatabaseName);
                }
                if (item.Name == nameof(CurrentSettings.ServerName))
                {
                    SettingsHelper.SetServerName(CurrentSettings.ServerName);
                }
            }

            SettingsHelper.Save();
        }
    }
}