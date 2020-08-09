using DailyQuestTimeScheduler.Views;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DailyQuestTimeScheduler.ViewModels
{
    /// <summary>
    /// ViewModel for main page of application .
    /// this class is using DB by using DataAccess class
    /// </summary>
    public class MainWindowViewModel:INotifyPropertyChanged
    {

        #region members
        private ObservableCollection<BoolTypeUserTask> boolTypeTaskList = new ObservableCollection<BoolTypeUserTask>();

        private UserTask selectedTaskHolder = null;

        private UserControl settingContent;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region properties
        public bool IsReseting { get; set; } = false;

        public ObservableCollection<BoolTypeUserTask> BoolTypeTaskList
        {
            get { return boolTypeTaskList; }
        }

        public List<NormalTaskHolder> TaskHolderList { get; set; }

        public SqliteDataAccess DBAccess { get; set; }

        public UserControl SettingContent
        {
            get { return settingContent; }
            set
            {
                settingContent = value;
                OnPropertyChanged();
            }
        }

        public UserTask SelectedTaskHolder
        {
            get { return selectedTaskHolder; }
            set
            {
                selectedTaskHolder = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand ResetAllTaskListAsyncCommand { get; set; }
        public ICommand DeleteSelectedTaskHolderAsyncCommand { get; set; }
        public ICommand CreatingTaskHolderSettingControlCommand { get; set; }

        #endregion     

        #region Constructor

        public MainWindowViewModel()
        {
            this.DBAccess = new SqliteDataAccessSqliteCon();

            this.ResetAllTaskListAsyncCommand = new AsyncCommand(ResetAllTaskListAsync, CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.DeleteSelectedTaskHolderAsyncCommand = new AsyncCommand(DeleteSeletedTaskHolderAsync, CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.CreatingTaskHolderSettingControlCommand = new DelegateCommand(AssignTaskHolderCreateControl);
            this.TaskHolderList = new List<NormalTaskHolder>();
        }

        public MainWindowViewModel(SqliteDataAccess dBAccess)
        {
            this.DBAccess = dBAccess;
            this.ResetAllTaskListAsyncCommand = new AsyncCommand(ResetAllTaskListAsync, CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.DeleteSelectedTaskHolderAsyncCommand = new AsyncCommand(DeleteSeletedTaskHolderAsync, CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.CreatingTaskHolderSettingControlCommand = new DelegateCommand(AssignTaskHolderCreateControl); 
            this.TaskHolderList = new List<NormalTaskHolder>();
        }

        #endregion

        #region Action

        public async Task DeleteSeletedTaskHolderAsync()
        {
            await this.DBAccess.DeleteTaskHolderAsync("Testing");

            if (selectedTaskHolder !=null)
            {
                await this.DBAccess.DeleteTaskHolderAsync(selectedTaskHolder.Title);
                await this.ResetAllTaskListAsync();
            }

        }

        public async Task ResetAllTaskListAsync()
        {
            try
            {
                IsReseting = true;

                await this.ResetBoolTypeTaskListAsync();
                await this.UpdateAllTaskListAsync();
                this.SettingContent = null;
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
        /// This usally happen when a day changes and an initial Step of the application.z
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
        
        public void AssignTaskHolderCreateControl(object obj)
        {
            if (SettingContent == null)
            {
                this.SettingContent = new TaskHolderSettingsControl() 
                    { 
                        OnAcceptButtonClick = AssignNewTaskHolderFromControlAsync,
                        OnCancelButtonClick = UnsetSettingControl
                    };
            }
        }

        public async Task AssignNewTaskHolderFromControlAsync(TaskHolder taskHolder)
        {
            try
            {
                if (taskHolder is NormalTaskHolder normalTaskHolder)
                    await AssignNewTaskHolder(normalTaskHolder);
                await ResetAllTaskListAsync();
            }
            finally
            {
                this.SettingContent = null;
            }
        }

        public void UnsetSettingControl()
        {
            this.SettingContent = null;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

        /// <summary>
        /// Create and Assign the bool tasks user did not finshed at past date
        /// </summary>
        /// <returns></returns>
        public async Task BringUnfinishedBoolTypeTasksAsync()
        {
            if (TaskHolderList.Count == 0)
                return;

            var today = (int)DateTime.Now.DayOfWeek;

            foreach (var taskHolder in TaskHolderList)
            {
                var totalDaysAfterInitDay = (DateTime.Now - taskHolder.InitTimeData).TotalDays;

                if (!taskHolder.IsRepeat)
                {
                    if (taskHolder.TaskDuration + 1 >= totalDaysAfterInitDay)
                    {
                        var boolTypeTask = await DBAccess.GetTaskOnSpecificDateAsync(taskHolder.Title, taskHolder.InitTimeData.ToString("G",
                            CultureInfo.CreateSpecificCulture("es-ES")));
                        if (boolTypeTask == null)
                            boolTypeTask = new BoolTypeUserTask(taskHolder.Title, taskHolder.InitTimeData);
                        else
                            boolTypeTask.Title = taskHolder.Title;
                        taskHolder.CurrentTaskList.Add(boolTypeTask);

                        boolTypeTask.OnDataChanged += UpdateCertainTask;
                        BoolTypeTaskList.Add(boolTypeTask);
                    }
                }
                else
                {

                    var checkUntilday = (taskHolder.TaskDuration < totalDaysAfterInitDay)
                        ? taskHolder.TaskDuration : totalDaysAfterInitDay;

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

                            boolTypeTask.OnDataChanged += UpdateCertainTask;
                            BoolTypeTaskList.Add(boolTypeTask);

                        }
                    }
                }
                
            }
        }


        private void UpdateCertainTask(UserTask task)
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

                    boolTypeTask.OnDataChanged += UpdateCertainTask;
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
