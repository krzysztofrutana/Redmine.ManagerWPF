using CommunityToolkit.Mvvm.DependencyInjection;
using Redmine.ManagerWPF.Desktop.Models.Tree;
using Redmine.ManagerWPF.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Helpers
{
    public static class TimeIntervalHelper
    {
        private static TimeIntervalsService _timeIntervalsService = Ioc.Default.GetRequiredService<TimeIntervalsService>();
        public static Stopwatch Timer { get; set; }
        public static TreeModel TreeNode {  get; set; }
        public static DateTime StartDateTime { get; set; }

        public static Task Start(TreeModel treeModel)
        {
            if (treeModel == null)
                throw new ArgumentNullException(nameof(treeModel), "Obiekt nie może być pusty");

            if(TreeNode != null && TreeNode.Id != treeModel.Id)
            {
                throw new Exception("Inny czas jest aktualnie liczony");
            }

            if(TreeNode != null && TreeNode.Type == treeModel.Type && TreeNode.Id == treeModel.Id)
            {
                throw new Exception("Czas dla tego obiektu jest aktualnie liczony");
            }

            TreeNode = treeModel;
            StartDateTime = DateTime.Now;
            Timer = new Stopwatch();
            Timer.Start();

            return Task.CompletedTask;
        }

        public static Task Stop(TreeModel treeModel)
        {
            if (TreeNode == null)
            {
                return Task.CompletedTask;
            }

            if (treeModel == null)
                throw new ArgumentNullException(nameof(treeModel), "Obiekt nie może być pusty");

            if (TreeNode != null && TreeNode.Type != treeModel.Type)
            {
                return Task.CompletedTask;
            }

            if (TreeNode != null && TreeNode.Type == treeModel.Type && TreeNode.Id != treeModel.Id)
            {
                return Task.CompletedTask;
            }

            Timer.Stop();
            var time = Timer.Elapsed.TotalSeconds;
            Timer.Restart();
            var endTime = StartDateTime.AddSeconds(time);

            TreeNode = null;


            return _timeIntervalsService.Create(treeModel, StartDateTime, endTime);

        }

        public static string CheckTime()
        {
            if(TreeNode != null && Timer.IsRunning)
            {
                return Timer.Elapsed.ToString(@"hh\:mm");
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
