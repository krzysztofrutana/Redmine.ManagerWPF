using CommunityToolkit.Mvvm.ComponentModel;

namespace Redmine.ManagerWPF.Desktop.Models.Issues
{
    public class FormModel : ObservableObject
    {
        private bool _done;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string Status { get; set; }
        public bool Done
        {
            get => _done; set
            {
                SetProperty(ref _done, value);
            }
        }
    }
}