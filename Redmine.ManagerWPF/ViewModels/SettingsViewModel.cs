using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Database;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Desktop.Models.Settings;
using Redmine.ManagerWPF.Helpers;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        #region Properties
        private SettingsModel _currentSettings;

        public SettingsModel CurrentSettings
        {
            get => _currentSettings;
            private set => SetProperty(ref _currentSettings, value);
        }

        private string _connectionStatusText;

        public string ConnectionStatusText
        {
            get => _connectionStatusText;

            private set => SetProperty(ref _connectionStatusText, value);
        }

        private bool? _connected;

        private bool? Connected
        {
            get => _connected;

            set => SetProperty(ref _connected, value);
        }
        #endregion

        #region Injections
        private readonly DatabaseManager _databaseManager;
        private readonly ILogger<SettingsViewModel> _logger;
        #endregion

        #region Commands
        public IRelayCommand SaveSettingsCommand { get; }
        public IRelayCommand ConnectionTestCommand { get; }
        public IRelayCommand CreateDatabaseCommand { get; }
        #endregion

        public SettingsViewModel()
        {
            _databaseManager = Ioc.Default.GetRequiredService<DatabaseManager>();
            _logger = Ioc.Default.GetLoggerForType<SettingsViewModel>();

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
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(ConnectionTest), ex.Message);
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
                switch (item.Name)
                {
                    case nameof(CurrentSettings.ApiKey):
                        SettingsHelper.SetApiKey(CurrentSettings.ApiKey);
                        break;
                    case nameof(CurrentSettings.Url):
                        SettingsHelper.SetUrl(CurrentSettings.Url);
                        break;
                    case nameof(CurrentSettings.DatabaseName):
                        SettingsHelper.SetDatabaseName(CurrentSettings.DatabaseName);
                        break;
                    case nameof(CurrentSettings.ServerName):
                        SettingsHelper.SetServerName(CurrentSettings.ServerName);
                        break;
                }
            }

            SettingsHelper.Save();
        }
    }
}