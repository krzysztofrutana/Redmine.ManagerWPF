using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Redmine.ManagerWPF.Desktop.Models.Tree
{
    public class TreeModel : ObservableObject
    {
        private bool _done;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Done
        {
            get => _done; 
            set
            {
                SetProperty(ref _done, value);
            }
        }

        public ObservableCollection<TreeModel> Children { get; set; } = new ObservableCollection<TreeModel>();

        public override string ToString()
        {
            return Name;
        }
    }
}