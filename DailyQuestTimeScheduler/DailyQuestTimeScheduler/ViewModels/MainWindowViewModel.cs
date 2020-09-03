﻿using DailyQuestTimeScheduler.Views;
using LiveCharts;
using LiveCharts.Wpf;
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
        public ICommand InsertTestingDataAsyncCommand { get; set; }

        #endregion     

        #region Constructor
        public SeriesCollection SeriesCollection { get; set; }

        public MainWindowViewModel()
        {
            this.DBAccess = new SqliteDataAccessSqliteCon();

            this.ResetAllTaskListAsyncCommand = new AsyncCommand(ResetAllTaskListAsync, 
                CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.DeleteSelectedTaskHolderAsyncCommand = new AsyncCommand(DeleteSeletedTaskHolderAsync,
                CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.InsertTestingDataAsyncCommand = new AsyncCommand(InsertTestingDataSetAsync,
                CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.CreatingTaskHolderSettingControlCommand = new DelegateCommand(AssignTaskHolderCreateControl);
            this.TaskHolderList = new List<NormalTaskHolder>();
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<double> { 3, 5, 7, 4 }
                },
                new ColumnSeries
                {
                    Values = new ChartValues<decimal> { 5, 6, 2, 7 }
                }
            };
        }

        public MainWindowViewModel(SqliteDataAccess dBAccess)
        {
            this.DBAccess = dBAccess;
            this.ResetAllTaskListAsyncCommand = new AsyncCommand(ResetAllTaskListAsync,
                CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
            this.DeleteSelectedTaskHolderAsyncCommand = new AsyncCommand(DeleteSeletedTaskHolderAsync,
                CanExcuteResetAllTaskList, new ErrorMeesageWhenException());
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

        #region ForTesing
        /// <summary>
        /// This class is for inserthing test data to see the live chart graph working or not.
        /// </summary>
        private async Task InsertTestingDataSetAsync()
        {
            Random rand = new Random();
            int taskDuration = 1;
            byte weeklyRepeatPattern = 0b01010101;
            int numOfDate = 30;
            List<Task> tasks = new List<Task>();
            DateTime numOfDayInTestingData = DateTime.Now - TimeSpan.FromDays(numOfDate);

            var today = (int)DateTime.Now.DayOfWeek;
            var taskHolder = new NormalTaskHolder("TestingData", "This is Testing", true, weeklyRepeatPattern, taskDuration, 3320, numOfDayInTestingData);

            await DBAccess.CreateNewTaskHolderAsync(taskHolder);

            for(int j = 0; j < numOfDate ; j++)
            {
                var checkingDayOfWeek = ((int)0b00000001 << ((today - j) % 7 + 7) % 7);
                if((weeklyRepeatPattern & checkingDayOfWeek) == checkingDayOfWeek)
                {
                    tasks.Add(DBAccess.UpsertUserTaskAsync(new BoolTypeUserTask("TestingData",
                        DateTime.Now - TimeSpan.FromDays(j)) 
                        {  
                            IsTaskDone = ( rand.NextDouble() > 0.5), 
                            TimeOfCompletionLocal = (DateTime.Now - TimeSpan.FromDays(j) - TimeSpan.FromHours(rand.Next(-5,3))).ToString("G", CultureInfo.CreateSpecificCulture("es-ES"))
                            //Set Random data.
                        }));
                }
            }
            await Task.WhenAll(tasks);

            await ResetAllTaskListAsync();
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
                        //dupli
                        await BringSpecificTaskOnTaskHolder(taskHolder, taskHolder.InitTimeData);
                        //else
                        //    boolTypeTask.Title = taskHolder.Title;
                }
                else
                {
                    var checkUntilDay = (taskHolder.TaskDuration < totalDaysAfterInitDay)
                        ? taskHolder.TaskDuration : totalDaysAfterInitDay;

                    for (int i = 1; i < checkUntilDay; i++)
                    {
                        //mod of negative number i to find dayOfWeek constraint
                        var checkingDay = ((int)0b00000001 << ((today - i) % 7 + 7) % 7);

                        if ((taskHolder.WeeklyRepeatPattern & checkingDay) == checkingDay)
                            await BringSpecificTaskOnTaskHolder(taskHolder, DateTime.Now + TimeSpan.FromDays(-i));
                    }
                }
            }
        }

        private async Task BringSpecificTaskOnTaskHolder(NormalTaskHolder taskHolder, DateTime date)
        {
            // dupli
            var boolTypeTask = await DBAccess.GetTaskOnSpecificDateAsync(taskHolder.Title, date.ToString("G",
                CultureInfo.CreateSpecificCulture("es-ES")));

            if (boolTypeTask == null)
                boolTypeTask = new BoolTypeUserTask(taskHolder.Title, date);

            taskHolder.CurrentTaskList.Add(boolTypeTask);

            boolTypeTask.OnDataChanged += UpdateCertainTask;
            BoolTypeTaskList.Add(boolTypeTask);

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
