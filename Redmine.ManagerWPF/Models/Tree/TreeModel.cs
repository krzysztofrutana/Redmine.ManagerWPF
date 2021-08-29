using System.Collections.ObjectModel;

namespace Redmine.ManagerWPF.Desktop.Models.Tree
{
    public class TreeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public ObservableCollection<TreeModel> Children { get; set; } = new ObservableCollection<TreeModel>();

        public override string ToString()
        {
            return Name;
        }
    }
}