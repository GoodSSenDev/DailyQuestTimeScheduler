using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
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
    public partial class TaskDataVisualizationControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string[] Days { get; set; }
        public string[] Weeks { get; set; }
        //0 null
        //1 didn;t finish  //2 3 finsh
        public ChartValues<HeatPoint> WeekCompletionView { get; set; }

        #region constructor
        public TaskDataVisualizationControl()
        {
            InitializeComponent();
            this.WeekCompletionView = new ChartValues<HeatPoint>
            {
                //setting defaut value
                new HeatPoint(0, 0, 0),
                new HeatPoint(1, 0, 0),
                new HeatPoint(2, 0, 0),
                new HeatPoint(3, 0, 0),
                new HeatPoint(4, 0, 0),
                new HeatPoint(5, 0, 0),
                new HeatPoint(6, 0, 0),

                new HeatPoint(0, 1, 0),
                new HeatPoint(1, 1, 0),
                new HeatPoint(2, 1, 0),
                new HeatPoint(3, 1, 0),
                new HeatPoint(4, 1, 0),
                new HeatPoint(5, 1, 0),
                new HeatPoint(6, 1, 0),

                new HeatPoint(0, 2, 0),
                new HeatPoint(1, 2, 0),
                new HeatPoint(2, 2, 0),
                new HeatPoint(3, 2, 0),
                new HeatPoint(4, 2, 0),
                new HeatPoint(5, 2, 0),
                new HeatPoint(6, 2, 0),

                new HeatPoint(0, 3, 0),
                new HeatPoint(1, 3, 0),
                new HeatPoint(2, 3, 0),
                new HeatPoint(3, 3, 0),
                new HeatPoint(4, 3, 0),
                new HeatPoint(5, 3, 0),
                new HeatPoint(6, 3, 0),

                new HeatPoint(0, 4, 0),
                new HeatPoint(1, 4, 0),
                new HeatPoint(2, 4, 0),
                new HeatPoint(3, 4, 0),
                new HeatPoint(4, 4, 0),
                new HeatPoint(5, 4, 0),
                new HeatPoint(6, 4, 0),
            };

            Weeks = new[]
            {
                "This Week",
                "Last Week",
                "3 Weeks back",
                "4 Weeks back",
                "5 Weeks back"
            };
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
        }

        #endregion

        public async Task InitialSetUp(TaskHolder taskHolder)
        {
            this.ResetHeatGraph();

            if (taskHolder is NormalTaskHolder nTaskHolder)
            {
                var dayOfWeek = (int)DateTime.Now.DayOfWeek;
                foreach (UserTask userTask in nTaskHolder.CurrentTaskList)
                {
                    if(userTask is BoolTypeUserTask boolTask)
                    {

                        var timeDifference = DateTime.Now.Date - boolTask.DateData.Date;
                        int week;
                       
                        if (timeDifference.Days <= dayOfWeek)
                            week = 0;
                        else
                        {
                            week = ((timeDifference.Days - (dayOfWeek + 1)) / 7)+1;
                        }
                        // since timeDifference value have current week

                        // diving time of completion by 5 
                        if (boolTask.IsTaskDone)
                            //var hourQuaterContraint = (boolTask.TimeOfCompletionLocalData.Hour / 6) + 2;
                            WeekCompletionView[(int)boolTask.TimeOfCompletionLocalData.DayOfWeek + (week * 7)].Weight = 3;
                        else
                            WeekCompletionView[(int)boolTask.TimeOfCompletionLocalData.DayOfWeek + (week * 7)].Weight = 1;
                    }
                }
                this.OnPropertyChanged("WeekCompletionView");
            }
        }

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ResetHeatGraph()
        {
            foreach(HeatPoint point in this.WeekCompletionView)
            {
                point.Weight = 0;
            }
        }

    }
}
