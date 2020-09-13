using DailyQuestTimeScheduler.Views;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

        private BoolTypeUserTask selectedTask = null;

        private UserControl settingContent;

        private UserControl dataVisualControl;

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

        public BoolTypeUserTask SelectedTask
        {
            get { return selectedTask; }
            set
            {
                selectedTask = value;

                if(dataVisualControl != null && value != null)
                {
                    if (dataVisualControl is TaskDataVisualizationControl visualControl)
                    {
                        visualControl.InitialSetUpAsync(selectedTask.ParentTaskHolder).FireAndForgetSafeAsync();
                    }
                }

                OnPropertyChanged();
            }
        }

        public UserControl SettingContent
        {
            get { return settingContent; }
            set
            {
                settingContent = value;
                OnPropertyChanged();
            }
        }

        public UserControl DataVisualControl
        {
            get { return dataVisualControl; }
            set
            {
                dataVisualControl = value;
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
            this.dataVisualControl = new TaskDataVisualizationControl();
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
            this.dataVisualControl = new TaskDataVisualizationControl();
        }

        #endregion

        #region Action

        public async Task DeleteSeletedTaskHolderAsync()
        {
            if (selectedTask !=null)
            {
                await this.DBAccess.DeleteTaskHolderAsync(selectedTask.ParentTaskHolder.Title);
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
            await this.AssignPastBoolTypeTasksAsync();
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
                taskHolder.Title = taskHolder.DisplayTitle.Replace(" ", "_");
                //check duplication and assign title for DB access to create table
                taskHolder.Title = this.GetTitleNameForDB(taskHolder.Title);

                if (taskHolder is NormalTaskHolder normalTaskHolder)
                    await AssignNewTaskHolder(normalTaskHolder);
                await ResetAllTaskListAsync();
            }
            finally
            {
                this.SettingContent = null;
            }
        }
        //for avoding duplication name of title for creating database tables witht title
        private string GetTitleNameForDB(string title)
        {
            string returnTitle = title;
            var list = this.TaskHolderList.Where(x => x.Title.StartsWith(title));
            
            if (list.Count() != 0)
            {
                for (int i = 1; ; i++)
                {
                    if (!list.Any(x => x.Title == title + i.ToString()))
                    {
                        returnTitle = title + i.ToString();
                        break;
                    }
                }
            }
            return returnTitle;
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
            byte weeklyRepeatPattern = 0b01111111;
            int numOfDate = 35;
            List<Task> tasks = new List<Task>();
            DateTime numOfDayInTestingData = DateTime.Now - TimeSpan.FromDays(numOfDate);

            var today = (int)DateTime.Now.DayOfWeek;
            var taskHolder = new NormalTaskHolder("TestingData",this.GetTitleNameForDB("TestingData"), "This is Testing", true, weeklyRepeatPattern, taskDuration, 3320, numOfDayInTestingData);

            await DBAccess.CreateNewTaskHolderAsync(taskHolder);

            for(int j = 0; j < numOfDate ; j++)
            {
                var checkingDayOfWeek = ((int)0b00000001 << ((today - j) % 7 + 7) % 7);
                if((weeklyRepeatPattern & checkingDayOfWeek) > 0)
                {
                    tasks.Add(DBAccess.UpsertUserTaskAsync(new BoolTypeUserTask(taskHolder.DisplayTitle,
                        DateTime.Now - TimeSpan.FromDays(j))
                    {
                        IsTaskDone = (rand.NextDouble() > 0.5),
                        Date = (DateTime.Now - TimeSpan.FromDays(j)).ToString("G", CultureInfo.CreateSpecificCulture("es-ES")),
                        TimeOfCompletionLocal = (DateTime.Now - TimeSpan.FromDays(j)).ToString("G", CultureInfo.CreateSpecificCulture("es-ES")),
                        ParentTaskHolder = taskHolder
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
        public async Task AssignPastBoolTypeTasksAsync()
        {
            if (TaskHolderList.Count == 0)
                return;

            foreach (var taskHolder in TaskHolderList)
            {
                var totalDaysAfterInitDay = (DateTime.Now - taskHolder.InitTimeData).TotalDays;

                if (!taskHolder.IsRepeat)
                {
                    if (taskHolder.TaskDuration + 1 >= totalDaysAfterInitDay)
                    {
                        var task = await BringSpecificTaskOnTaskHolderAsync(taskHolder, taskHolder.InitTimeData);
                        if(task is BoolTypeUserTask boolTask)
                            this.BoolTypeTaskList.Add(boolTask);
                    }
                        //else
                        //    boolTypeTask.Title = taskHolder.Title;
                }
                else
                {
                    await AssignPastWeeklyTaskAsync(taskHolder, totalDaysAfterInitDay);
                }
            }
        }

        private async Task AssignPastWeeklyTaskAsync(NormalTaskHolder taskHolder, double totalDaysAfterInitDay)
        {
            int today = (int)DateTime.Now.DayOfWeek;
            var checkUntilDay = (taskHolder.TaskDuration < totalDaysAfterInitDay)
                ? taskHolder.TaskDuration : totalDaysAfterInitDay;

            var tasks = Enumerable.Range(1, (int)checkUntilDay - 1).Select(async (i) =>
                {
                    //mod of negative number i to find dayOfWeek constraint
                    var checkingDay = ((int)0b00000001 << ((today - i) % 7 + 7) % 7);

                    if ((taskHolder.WeeklyRepeatPattern & checkingDay) > 0)
                    {
                        var task = await BringSpecificTaskOnTaskHolderAsync(taskHolder, DateTime.Now + TimeSpan.FromDays(-i));
                        if (task is BoolTypeUserTask boolTask)
                        {
                            this.BoolTypeTaskList.Add(boolTask);
                        }
                    }
                });

            var totalDaysInFourWeeks = 28;
            var visualDate = ((today + 1) + totalDaysInFourWeeks < totalDaysAfterInitDay)
                ? (today + 1) + totalDaysInFourWeeks : totalDaysAfterInitDay;

            var tasks2 = Enumerable.Range((int)checkUntilDay, (int)visualDate - 1).Select(async (i) =>
            {
                //mod of negative number i to find dayOfWeek constraint
                var checkingDay = ((int)0b00000001 << ((today - i) % 7 + 7) % 7);

                if ((taskHolder.WeeklyRepeatPattern & checkingDay) > 0)
                {
                    var task = await BringSpecificTaskOnTaskHolderAsync(taskHolder, DateTime.Now + TimeSpan.FromDays(-i));
                    if ((taskHolder.WeeklyRepeatPattern & checkingDay) > 0)
                        await BringSpecificTaskOnTaskHolderAsync(taskHolder, (DateTime.Now + TimeSpan.FromDays(-i)));
                }
            });

            await Task.WhenAll(tasks);
            await Task.WhenAll(tasks2);
        }

        private async Task<UserTask> BringSpecificTaskOnTaskHolderAsync(NormalTaskHolder taskHolder, DateTime date)
        {
            var boolTypeTask = await DBAccess.GetTaskOnSpecificDateAsync(taskHolder.Title, date.ToString("G",
                CultureInfo.CreateSpecificCulture("es-ES")));

            if (boolTypeTask == null)
                boolTypeTask = new BoolTypeUserTask(taskHolder.DisplayTitle, date);

            boolTypeTask.DisplayTitle = taskHolder.DisplayTitle;
            boolTypeTask.ParentTaskHolder = taskHolder;
            boolTypeTask.OnDataChanged += UpdateCertainTask;
            taskHolder.CurrentTaskList.Add(boolTypeTask);

            return boolTypeTask;
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

                if ((taskHolder.WeeklyRepeatPattern & checkingDay) > 0)
                {
                    var boolTypeTask = await DBAccess.GetTaskOnSpecificDateAsync(taskHolder.Title, DateTime.Now.ToString("G",
                        CultureInfo.CreateSpecificCulture("es-ES")));

                    if (boolTypeTask == null)
                        boolTypeTask = new BoolTypeUserTask(taskHolder.DisplayTitle);
                    else
                        boolTypeTask.DisplayTitle = taskHolder.DisplayTitle;

                    taskHolder.CurrentTaskList.Add(boolTypeTask);
                    boolTypeTask.ParentTaskHolder = taskHolder;
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
