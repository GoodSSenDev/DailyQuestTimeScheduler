using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DailyQuestTimeScheduler.ViewModels
{
    public class MainWindowViewModel
    {
        public bool IsReseting { get; set; } = false;

        public ICommand ResetAllTaskListAsyncCommand { get; set; }


        public SqliteDataAccess DBAccess { get; set; }

        private ObservableCollection<BoolTypeUserTask> boolTypeTaskList = new ObservableCollection<BoolTypeUserTask>();
        public ObservableCollection<BoolTypeUserTask> BoolTypeTaskList
        {
            get { return boolTypeTaskList; }
        }

        public List<NormalTaskHolder> TaskHolderList { get; set; }

        #region Constructor

        public MainWindowViewModel()
        {
            this.DBAccess = new SqliteDataAccessSqliteCon();

            ResetAllTaskListAsyncCommand = new AsyncCommand(ResetAllTaskListAsync, CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
        }

        public MainWindowViewModel(SqliteDataAccess dBAccess)
        {
            this.DBAccess = dBAccess;
        }

        #endregion


        #region Action

        public async Task ResetAllTaskListAsync()
        {
            try
            {
                IsReseting = true;

                await this.UpdateAllTaskListAsync();
                await ResetBoolTypeTaskListAsync();
                await this.UpdateAllTaskListAsync();
            }
            finally
            {
                IsReseting = false;
            }
        }

        public bool CanExcuteResetAllTaskList()
        {
            return !(IsReseting);
        }

        public async Task<List<NormalTaskHolder>> GetTaskHolderListAsync()
        {
            return TaskHolderList = await DBAccess.GetTaskHolderListAsync();
        }

        /// <summary>
        /// Reset All Task List 
        /// This usally happen when a day changes and an initial Step of the application.
        /// </summary>
        /// <returns></returns>
        private async Task ResetBoolTypeTaskListAsync()
        {
            TaskHolderList.Clear();
            boolTypeTaskList.Clear();

            await this.BringUnfinishedBoolTypeTasksAsync();
            await this.AssignTodaysBoolTaskAsync();
        }


        #endregion

        #region Fundamental DBAccess Class

        public async Task AssignNewTaskHolder(NormalTaskHolder taskHolder)
        {
            await DBAccess.CreateNewTaskHolderAsync(taskHolder);
        }

        public async Task BringUnfinishedBoolTypeTasksAsync()
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
        public async Task AssignTodaysBoolTaskAsync()
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
        public async Task UpdateAllTaskListAsync()
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
