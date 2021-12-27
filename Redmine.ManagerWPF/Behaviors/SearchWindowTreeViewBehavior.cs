using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Redmine.ManagerWPF.Desktop.Behaviors
{
    public class SearchWindowTreeViewBehavior : Behavior<TreeView>
    {
        protected override void OnAttached()
        {
            AssociatedObject.SelectedItemChanged += AssociatedObject_SelectedItemChanged;
        }

        private void AssociatedObject_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tv = sender as TreeView;
            if (tv == null) return;
            var vm = tv.DataContext as SearchWindowViewModel;
            if (vm == null) return;
            vm.SelectedNode = tv.SelectedItem as TreeModel;
        }
    }
}