using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DailyQuestTimeScheduler.ViewModels
{
    public class MainWindowViewModel
    {
        public SqliteDataAccess DBAccess { get; set; }

        private ObservableCollection<BoolTypeUserTask> boolTypeTaskList = new ObservableCollection<BoolTypeUserTask>();
        public ObservableCollection<BoolTypeUserTask> BoolTypeTaskList
        {
            get { return boolTypeTaskList; }
        }

        public List<NormalTaskHolder> TaskHolderList { get; set; }

        public MainWindowViewModel()
        {
            this.DBAccess = new SqliteDataAccessSqliteCon();
        }

        public MainWindowViewModel(SqliteDataAccess dBAccess)
        {
            this.DBAccess = dBAccess;
        }

        public async Task<List<NormalTaskHolder>> GetTaskHolderListAsync()
        {
            return TaskHolderList = await DBAccess.GetTaskHolderListAsync();
        }

        public async Task BringUnfinishedTasks()
        {
            if (TaskHolderList.Count == 0)
                return;

            var today = (int)DateTime.Now.DayOfWeek;

            foreach (var taskHolder in TaskHolderList)
            {
                var daysFromTaskHolderInitDay = (taskHolder.InitTimeData - DateTime.Now).TotalDays;

                var checkUntilday = (taskHolder.TaskDuration > daysFromTaskHolderInitDay)
                    ? taskHolder.TaskDuration : daysFromTaskHolderInitDay;

                for (int i = 1; i < checkUntilday; i++)
                {
                    var checkingDay = (0b00000001 << (today - i % 7));
                     
                    if ((taskHolder.WeeklyRepeatPattern & checkingDay) == checkingDay)
                    {
                        var dateTimeAtThatTime = DateTime.Now + TimeSpan.FromDays(-i);
                        var boolTypeTask = await DBAccess.GetTaskOnCertainDateAsync(taskHolder.Title, dateTimeAtThatTime.ToString("G",
                            CultureInfo.CreateSpecificCulture("es-ES")));

                        if (boolTypeTask == null)
                            boolTypeTask = new BoolTypeUserTask(taskHolder.Title, dateTimeAtThatTime);
                        else
                            boolTypeTask.Title = taskHolder.Title;

                        taskHolder.CurrentTaskList.Add(boolTypeTask);

                        BoolTypeTaskList.Add(boolTypeTask);
                    }
                }
                
            }

        }

        // update the task that is checked using command 

    }
}
