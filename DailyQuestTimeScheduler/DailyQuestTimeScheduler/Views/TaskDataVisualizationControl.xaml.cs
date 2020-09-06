using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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
        public double PersentageOfComplete { get; set; }
        //0 null
        //1 didn;t finish  //2 3 finsh
        public ChartValues<HeatPoint> WeekCompletionView { get; set; }

        public SeriesCollection WeekTaskCompletionVarGraph { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        #region constructor
        public TaskDataVisualizationControl()
        {
            InitializeComponent();

            WeekTaskCompletionVarGraph = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    Values = new ChartValues<double> {0, 1, 2, 3, 4},
                    
                    StackMode = StackMode.Values, // this is not necessary, values is the default stack mode
                   
                    DataLabels = true
                }
            };

            Labels = new[] { "5 Weeks back", "4 Weeks back", "3 Weeks back", "Last Week", "This Week" };
            Formatter = value => value + " %";

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
                int[] weeksTaskFinishCount = new int[5];
                int i = 0;
                int addCount = 0;
                for (int weekCount = 0; weekCount < 5; weekCount++)
                {
                    for (int k = 0; k < 7; k++, i++)
                    {
                        if (this.WeekCompletionView[i].Weight > 1)
                            weeksTaskFinishCount[weekCount]++;
                    }
                }

                WeekTaskCompletionVarGraph[0].Values[0] = (weeksTaskFinishCount[4] == 0) ? 0 : ((double)(int)((weeksTaskFinishCount[4] / 7d) * 100));
                WeekTaskCompletionVarGraph[0].Values[1] = (weeksTaskFinishCount[3] == 0) ? 0 : ((double)(int)((weeksTaskFinishCount[3] / 7d) * 100));
                WeekTaskCompletionVarGraph[0].Values[2] = (weeksTaskFinishCount[2] == 0) ? 0 : ((double)(int)((weeksTaskFinishCount[2] / 7d) * 100));
                WeekTaskCompletionVarGraph[0].Values[3] = (weeksTaskFinishCount[1] == 0) ? 0 : ((double)(int)((weeksTaskFinishCount[1] / 7d) * 100));
                WeekTaskCompletionVarGraph[0].Values[4] = (weeksTaskFinishCount[0] == 0) ? 0 : ((double)(int)((weeksTaskFinishCount[0] / 7d) * 100));

                for(int c = 1; c < 5; c++)
                {
                    addCount = addCount + weeksTaskFinishCount[c]; 
                }
                PersentageOfComplete = (addCount == 0) ? 0 : ((double)(int)((addCount / 28d) * 100));


                this.OnPropertyChanged("PersentageOfComplete");
                this.OnPropertyChanged("WeekCompletionView");
                this.OnPropertyChanged("WeekTaskCompletionVarGraph");

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
