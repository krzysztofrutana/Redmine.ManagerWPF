using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Redmine.ManagerWPF.Data;
using Redmine.ManagerWPF.Desktop.Models.Settings;
using Redmine.ManagerWPF.Helpers;
using System;

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
                _currentSettings = value;
                OnPropertyChanged(nameof(CurrentSettings));
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
                _connectionStatusText = value;
                OnPropertyChanged(nameof(ConnectionStatusText));
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
                _connected = value;
                OnPropertyChanged(nameof(Connected));
            }
        }

        public IRelayCommand SaveSettingsCommand { get; }
        public IRelayCommand ConnectionTestCommand { get; }
        public IRelayCommand CreateDatabaseCommand { get; }

        private readonly Context _context;

        public SettingsViewModel()
        {
            _context = Ioc.Default.GetRequiredService<Context>();

            CurrentSettings = new SettingsModel();
            LoadCurrentSettings();
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            ConnectionTestCommand = new RelayCommand(ConnectionTest);
            CreateDatabaseCommand = new RelayCommand(CreateDatabase);
        }

        private void ConnectionTest()
        {
            SaveSettings();
            if (_context.Database.CanConnect())
            {
                ConnectionStatusText = "Połączono";
                Connected = true;
            }
            else
            {
                ConnectionStatusText = "Błąd, spróbuj ponownie";
                Connected = false;
            }
        }
        private void CreateDatabase()
        {
            SaveSettings();
            if (!_context.Database.CanConnect())
            {
                try
                {
                    _context.Database.EnsureCreated();
                    ConnectionStatusText = "Połączono";
                    Connected = true;
                }
                catch (Exception ex)
                {
                    ConnectionStatusText = "Błąd, tworzenia bazy";
                    Connected = false;
                }
            }
            else
            {
                ConnectionStatusText = "Baza danych już istnieje";
                Connected = true;
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

            _context.Database.GetDbConnection().ConnectionString = String.Format(@"Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", CurrentSettings.ServerName, CurrentSettings.DatabaseName);
        }
    }
}