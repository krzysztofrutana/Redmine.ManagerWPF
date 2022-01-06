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
    public class CreateDatabaseBackupViewModel : ObservableObject
    {

        private string _folderPath;

        public string FolderPath
        {
            get { return _folderPath; }
            set { SetProperty(ref _folderPath, value); }
        }

        private string _information;

        public string Information
        {
            get { return _information; }
            set { SetProperty(ref _information, value); }
        }

        public IAsyncRelayCommand<ICloseable> CloseWindowCommand { get; }
        public IAsyncRelayCommand CreateBackupCommand { get; }
        public IRelayCommand OpenFolderPickerDialogCommand { get; }

        private readonly IMessageBoxService _messageBoxService;
        private readonly IMapper _mapper;

        public CreateDatabaseBackupViewModel()
        {
            _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
            _mapper = Ioc.Default.GetRequiredService<IMapper>();

            CloseWindowCommand = new AsyncRelayCommand<ICloseable>(CloseWindow);
            CreateBackupCommand = new AsyncRelayCommand(CreateBackupAsync);
            OpenFolderPickerDialogCommand = new RelayCommand(OpenFolderPickerDialog);

        }

        private void OpenFolderPickerDialog()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    FolderPath = dialog.SelectedPath;
                }
            }
        }

        private async Task CreateBackupAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(FolderPath))
                {

                    var connectionString = "";
                    var databaseName = SettingsHelper.GetDatabaseName();
                    var server = SettingsHelper.GetServerName();

                    if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(server))
                    {
                        connectionString = $"Server={server};Database={databaseName};Trusted_Connection=True;";
                        var fileName = string.Format("{0}_{1}.bak", databaseName, DateTime.Now.ToString("dd-MM-yyyy-HH-mm"));

                        using (var connection = new SqlConnection(connectionString))
                        {
                            var query = String.Format("BACKUP DATABASE [{0}] TO DISK='{1}'", databaseName, Path.Combine(FolderPath, fileName));

                            using (var command = new SqlCommand(query, connection))
                            {
                                connection.Open();
                                await command.ExecuteNonQueryAsync();
                            }

                            Information = $"Wykonano kopię do pliku {fileName}";

                            _messageBoxService.ShowInformationBox("Backup zakończony powodzeniem", "Sukces");
                        }
                    }
                }
                else
                {
                    _messageBoxService.ShowWarningInfoBox("Nie wybrano folderu, backup niemożliwy", "Uwaga");
                }

            }
            catch (Exception ex)
            {
                _messageBoxService.ShowWarningInfoBox(ex.Message, "Błąd przy tworzeniu kopii");
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