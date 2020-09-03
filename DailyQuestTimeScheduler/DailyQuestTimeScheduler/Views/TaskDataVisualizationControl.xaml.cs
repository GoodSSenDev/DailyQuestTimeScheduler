using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DailyQuestTimeScheduler.Views
{
    /// <summary>
    /// Interaction logic for TaskDataVisualizationControl.xaml
    /// Set up the various of chart that visualise a TaskHolder Data
    /// </summary>
    public partial class TaskDataVisualizationControl : UserControl
    {
        public string[] Days { get; set; }
        public string[] Weeks { get; set; }
        //0 null
        //1 didn;t finish // 2 // 3 // 4// 5 //6
        public ChartValues<HeatPoint> WeekCompletionView { get; set; } = new ChartValues<HeatPoint>();


        #region constructor
        public TaskDataVisualizationControl()
        {
            InitializeComponent();
            Days = new[]
            {
                "Sunday",
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday"
            };

            Weeks = new[]
            {
                "This Week",
                "Last Week",
                "3 Weeks back",
                "4 Weeks back",
                "5 Weeks back"
            };
        }
        #endregion

        public void InitialSetUp(TaskHolder taskHolder)
        {
            if(taskHolder is NormalTaskHolder nTaskHolder)
            {
                var dayOfWeek = (int)DateTime.Now.DayOfWeek;
                foreach (UserTask userTask in nTaskHolder.CurrentTaskList)
                {
                    if(userTask is BoolTypeUserTask boolTask)
                    {

                        var timeDifference = DateTime.Now - boolTask.DateData;
                        int week;
                        //
                        if (timeDifference.Days < dayOfWeek)
                            week = 0;
                        else
                            week = (timeDifference.Days / 7);
                        // since timeDifference value have current week

                        // diving time of completion by 5 
                        if (boolTask.IsTaskDone)
                            //var hourQuaterContraint = (boolTask.TimeOfCompletionLocalData.Hour / 6) + 2;
                            WeekCompletionView.Add(new HeatPoint(week, (int)boolTask.TimeOfCompletionLocalData.DayOfWeek, 
                                (boolTask.TimeOfCompletionLocalData.Hour / 6) + 2));
                        else
                            WeekCompletionView.Add(new HeatPoint(week, (int)boolTask.TimeOfCompletionLocalData.DayOfWeek, 1));
                    }
                }
            }
        }




    }
}
