using CommunityToolkit.Mvvm.ComponentModel;

namespace Redmine.ManagerWPF.Desktop.Models.Settings
{
    public class SettingsModel : ObservableObject
    {
        private string _serverName;

        public string ServerName
        {
            get
            {
                return _serverName;
            }

            set
            {
                _serverName = value;
                OnPropertyChanged(nameof(ServerName));
            }
        }

        private string _databaseName;

        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }

            set
            {
                _databaseName = value;
                OnPropertyChanged(nameof(DatabaseName));
            }
        }

        private string _apiKey;

        public string ApiKey
        {
            get
            {
                return _apiKey;
            }

            set
            {
                _apiKey = value;
                OnPropertyChanged(nameof(ApiKey));
            }
        }

        private string _url;

        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                _url = value;
                OnPropertyChanged(nameof(Url));
            }
        }
    }
}