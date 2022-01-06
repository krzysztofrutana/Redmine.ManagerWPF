using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data.Enums;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Desktop.Messages;
using Redmine.ManagerWPF.Desktop.Services;
using Redmine.ManagerWPF.Helpers;
using Redmine.ManagerWPF.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.ViewModels
{
    public class RestoreDatabaseBackupViewModel : ObservableObject
    {

        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }

        private string _information;

        public string Information
        {
            get { return _information; }
            set { SetProperty(ref _information, value); }
        }

        public IAsyncRelayCommand<ICloseable> CloseWindowCommand { get; }
        public IAsyncRelayCommand RestoreBackupCommand { get; }
        public IRelayCommand OpenFilePickerDialogCommand { get; }

        private readonly IMessageBoxService _messageBoxService;
        private readonly IMapper _mapper;

        public RestoreDatabaseBackupViewModel()
        {
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();

            CloseWindowCommand = new AsyncRelayCommand<ICloseable>(CloseWindow);
            RestoreBackupCommand = new AsyncRelayCommand(RestoreBackupAsync);
            OpenFilePickerDialogCommand = new RelayCommand(OpenFilePickerDialog);

        }

        private void OpenFilePickerDialog()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "SQLServer bak files (*.bak)|*.bak";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    FilePath = dialog.FileName;
                }
            }
        }

        private async Task RestoreBackupAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(FilePath))
                {

                    var connectionString = "";
                    var databaseName = SettingsHelper.GetDatabaseName();
                    var server = SettingsHelper.GetServerName();

                    if (!string.IsNullOrWhiteSpace(server))
                    {
                        connectionString = $"Server={server};Database=master;Trusted_Connection=True;";

                        using (var connection = new SqlConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            var queryCloseAllConnections = String.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", databaseName, FilePath);

                            using (var command = new SqlCommand(queryCloseAllConnections, connection))
                            {
                                await command.ExecuteNonQueryAsync();
                            }

                            var queryRestoreBackup = String.Format("RESTORE DATABASE [{0}] FROM DISK='{1}' WITH REPLACE", databaseName, FilePath);

                            using (var command = new SqlCommand(queryRestoreBackup, connection))
                            {
                                await command.ExecuteNonQueryAsync();
                            }

                            var restoreAccesToDatabase = String.Format("ALTER DATABASE [{0}] SET MULTI_USER", databaseName, FilePath);

                            using (var command = new SqlCommand(restoreAccesToDatabase, connection))
                            {
                                await command.ExecuteNonQueryAsync();
                            }

                            await connection.CloseAsync();

                            Information = $"Przywrócono kopię z pliku {Path.GetFileName(FilePath)}. W celu pobrania nadpisanych danych proszę zrestartować aplikację";

                            _messageBoxService.ShowInformationBox("Przywracanie zakończone powodzeniem, Zrestartuj aplikację", "Sukces");
                        }
                    }
                }
                else
                {
                    _messageBoxService.ShowWarningInfoBox("Nie wybrano pliku, przywrócenie niemożliwe", "Uwaga");
                }
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Błąd przy przywracaniu kopii");
            }

        }

        private Task CloseWindow(ICloseable window)
        {
            if (window != null)
            {
                window.Close();
            }

            return Task.CompletedTask;
        }
    }
}