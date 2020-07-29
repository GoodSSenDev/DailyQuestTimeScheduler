using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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

        #region DBAccess Class

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
                    //For mod of negative number i mode 2 times 
                    var checkingDay = ((int)0b00000001 << ((today - i) % 7 + 7) % 7);
                     
                    if ((taskHolder.WeeklyRepeatPattern & checkingDay) == checkingDay)
                    {
                        var dateTimeAtThatTime = DateTime.Now + TimeSpan.FromDays(-i);
                        var boolTypeTask = await DBAccess.GetTaskOnSpecificDateAsync(taskHolder.Title, dateTimeAtThatTime.ToString("G",
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

        /// <summary>
        /// 
        /// Check the DB and if the task not exist for today then  Assign new Task To Task List
        /// </summary>
        /// <returns></returns>
        public async Task AssignTodaysTaskAsync()
        {
            if (TaskHolderList.Count == 0)
                return;

            var today = (int)DateTime.Now.DayOfWeek;

            foreach (var taskHolder in TaskHolderList)
            {
                var checkingDay = (0b00000001 << (today));

                if ((taskHolder.WeeklyRepeatPattern & checkingDay) == checkingDay)
                {
                    var boolTypeTask = await DBAccess.GetTaskOnSpecificDateAsync(taskHolder.Title, DateTime.Now.ToString("G",
                        CultureInfo.CreateSpecificCulture("es-ES")));

                    if (boolTypeTask == null)
                        boolTypeTask = new BoolTypeUserTask(taskHolder.Title);
                    else
                        boolTypeTask.Title = taskHolder.Title;

                    taskHolder.CurrentTaskList.Add(boolTypeTask);

                    BoolTypeTaskList.Add(boolTypeTask);
                }

            }
        }

        /// <summary>
        /// Upsert Every Tasks In TaskHolder .
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAllTasksAsync()
        {
            if (TaskHolderList.Count == 0)
                return;

            foreach(var taskHolder in TaskHolderList)
            {
                for(int i = 0; i< taskHolder.CurrentTaskList.Count; i++)
                {
                    await DBAccess.UpsertUserTaskAsync(taskHolder.CurrentTaskList[i]);
                }
            }

        }

        /// <summary>
        /// Just Upsert The userTask (insert and if same date exist then Update on that row) 
        /// </summary>
        /// <param name="userTask"></param>
        /// <returns></returns>
        public async Task UpdateTaskAsync(BoolTypeUserTask userTask)
        {
            await DBAccess.UpsertUserTaskAsync(userTask);
        }
        #endregion



    }
}
