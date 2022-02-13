using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Desktop.Extensions;
using Redmine.ManagerWPF.Helpers;
using Redmine.ManagerWPF.Helpers.Interfaces;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class CreateDatabaseBackupViewModel : ObservableObject
    {

        private string _folderPath;

        public string FolderPath
        {
            get => _folderPath;
            private set => SetProperty(ref _folderPath, value);
        }

        private string _information;

        public string Information
        {
            get => _information;
            private set => SetProperty(ref _information, value);
        }

        public IAsyncRelayCommand<ICloseable> CloseWindowCommand { get; }
        public IAsyncRelayCommand CreateBackupCommand { get; }
        public IRelayCommand OpenFolderPickerDialogCommand { get; }

        private readonly IMessageBoxService _messageBoxService;
        private readonly ILogger<CreateDatabaseBackupViewModel> _logger;

        public CreateDatabaseBackupViewModel()
        {
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _logger = Ioc.Default.GetLoggerForType<CreateDatabaseBackupViewModel>();

            CloseWindowCommand = new AsyncRelayCommand<ICloseable>(CloseWindow);
            CreateBackupCommand = new AsyncRelayCommand(CreateBackupAsync);
            OpenFolderPickerDialogCommand = new RelayCommand(OpenFolderPickerDialog);

        }

        private void OpenFolderPickerDialog()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FolderPath = dialog.SelectedPath;
            }
        }

        private async Task CreateBackupAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(FolderPath))
                {
                    var databaseName = SettingsHelper.GetDatabaseName();
                    var server = SettingsHelper.GetServerName();

                    if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(server))
                    {
                        var connectionString = $"Server={server};Database={databaseName};Trusted_Connection=True;";
                        var fileName = $"{databaseName}_{DateTime.Now:dd-MM-yyyy-HH-mm}.bak";

                        await using var connection = new SqlConnection(connectionString);
                        var query = $"BACKUP DATABASE [{databaseName}] TO DISK='{Path.Combine(FolderPath, fileName)}'";

                        await using (var command = new SqlCommand(query, connection))
                        {
                            connection.Open();
                            await command.ExecuteNonQueryAsync();
                        }

                        Information = $"Wykonano kopię do pliku {fileName}";

                        _messageBoxService.ShowInformationBox("Backup zakończony powodzeniem", "Sukces");
                    }
                }
                else
                {
                    _messageBoxService.ShowWarningInfoBox("Nie wybrano folderu, backup niemożliwy", "Uwaga");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(CreateBackupAsync), ex.Message);
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Błąd przy tworzeniu kopii");
            }

        }

        private Task CloseWindow(ICloseable window)
        {
            window?.Close();

            return Task.CompletedTask;
        }
    }
}