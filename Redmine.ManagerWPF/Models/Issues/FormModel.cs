using CommunityToolkit.Mvvm.ComponentModel;

namespace Redmine.ManagerWPF.Desktop.Models.Issues
{
    public class FormModel : ObservableObject
    {
        private bool _done;
        private string _name;
        private string _description;

        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string Link { get; set; }
        public string Status { get; set; }
        public bool Done { get => _done; set => SetProperty(ref _done, value); }
    }
}