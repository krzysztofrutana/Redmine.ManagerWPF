using CommunityToolkit.Mvvm.ComponentModel;

namespace Redmine.ManagerWPF.Desktop.Models.Comments
{
    public class FormModel : ObservableObject
    {
        private bool _done;
        private string _status;

        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public string Text { get; set; }
        public string Link { get; set; }
        public string Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
            }
        }
        public bool Done
        {
            get => _done; set
            {
                SetProperty(ref _done, value);
            }
        }
    }
}