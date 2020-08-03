using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DailyQuestTimeScheduler.ViewModels
{
    /// <summary>
    /// ViewModel for main page of application .
    /// </summary>
    public class MainWindowViewModel
    {

        #region members
        private ObservableCollection<BoolTypeUserTask> boolTypeTaskList = new ObservableCollection<BoolTypeUserTask>();

        UserTask selectedTask;

        #endregion

        #region properties
        public bool IsReseting { get; set; } = false;

        public ObservableCollection<BoolTypeUserTask> BoolTypeTaskList
        {
            get { return boolTypeTaskList; }
        }

        public List<NormalTaskHolder> TaskHolderList { get; set; }

        public SqliteDataAccess DBAccess { get; set; }

        public UserTask SelectedTask
        {
            get { return selectedTask; }
            set
            {

            }
        }

        #endregion

        #region Commands

        public ICommand ResetAllTaskListAsyncCommand { get; set; }
        public ICommand AddTaskAsyncCommand { get; set; }
        public ICommand TestingDeleteAsyncCommand { get; set; }

        #endregion     

        #region Constructor

        public MainWindowViewModel()
        {
            this.DBAccess = new SqliteDataAccessSqliteCon();

            this.ResetAllTaskListAsyncCommand = new AsyncCommand(ResetAllTaskListAsync, CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.AddTaskAsyncCommand = new AsyncCommand(AddTaskHolderAsync, CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.TestingDeleteAsyncCommand = new AsyncCommand(TestingDeleteAsync, CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.TaskHolderList = new List<NormalTaskHolder>();
        }

        public MainWindowViewModel(SqliteDataAccess dBAccess)
        {
            this.DBAccess = dBAccess;
            this.TaskHolderList = new List<NormalTaskHolder>();
        }

        #endregion

        #region Action
        public async Task AddTaskHolderAsync()
        {

            await this.AssignNewTaskHolder(new NormalTaskHolder() { 
                Title = "Testing",
                Description = "This is testing1",
                IsRepeat = true,
                TaskDuration = 1,
                WeeklyRepeatPattern = 0b00101010
            });
            await this.AssignNewTaskHolder(new NormalTaskHolder()
            {
                Title = "Testing2",
                Description = "This is testing2",
                IsRepeat = true,
                TaskDuration = 1,
                WeeklyRepeatPattern = 0b01010101
            });
        }

        public async Task TestingDeleteAsync()
        {
            await this.DBAccess.DeleteTaskHolderAsync("Testing");
            await this.DBAccess.DeleteTaskHolderAsync("Testing2");
        }


        public async Task ResetAllTaskListAsync()
        {
            try
            {
                IsReseting = true;

                await this.ResetBoolTypeTaskListAsync();
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

        /// <summary>
        /// Reset All Task List 
        /// This usally happen when a day changes and an initial Step of the application.
        /// </summary>
        /// <returns></returns>
        private async Task ResetBoolTypeTaskListAsync()
        {
            TaskHolderList.Clear();
            BoolTypeTaskList.Clear();

            await this.GetTaskHolderListAsync();
            await this.BringUnfinishedBoolTypeTasksAsync();
            await this.AssignTodaysBoolTaskAsync();
        }


        #endregion

        #region Fundamental DBAccess Class
        public async Task<List<NormalTaskHolder>> GetTaskHolderListAsync()
        {
            return TaskHolderList = await DBAccess.GetTaskHolderListAsync();
        }

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
                var daysFromTaskHolderInitDay = (DateTime.Now - taskHolder.InitTimeData).TotalDays;

                var checkUntilday = (taskHolder.TaskDuration < daysFromTaskHolderInitDay)
                    ? taskHolder.TaskDuration : daysFromTaskHolderInitDay;

                for (int i = 1; i < checkUntilday; i++)
                {
                    //mod of negative number i
                    var checkingDay = ((int)0b00000001 << ((today - i) % 7 + 7) % 7);
                     
                    if ((taskHolder.WeeklyRepeatPattern & checkingDay) == checkingDay)
                    {
                        var dateTimeAtThatTime = DateTime.Now + TimeSpan.FromDays(-i);
                        var boolTypeTask = await DBAccess.GetTaskOnSpecificDateAsync(taskHolder.Title, dateTimeAtThatTime.ToString("G",
                            CultureInfo.CreateSpecificCulture("es-ES")));

                        if (boolTypeTask == null)
                            boolTypeTask = new BoolTypeUserTask(taskHolder.Title, dateTimeAtThatTime);

                        taskHolder.CurrentTaskList.Add(boolTypeTask);

                        boolTypeTask.OnDataChanged += UpdateCetainTask;
                        BoolTypeTaskList.Add(boolTypeTask);

                    }
                }
                
            }
        }

        private void UpdateCetainTask(UserTask task)
        {
            if (task is BoolTypeUserTask t)
            {
                UpdateTaskAsync(t).FireAndForgetSafeAsync(new ErrorMeesageWhenException());
                return;
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

                    boolTypeTask.OnDataChanged += UpdateCetainTask;
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
                for(int i = 0; i < taskHolder.CurrentTaskList.Count; i++)
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
