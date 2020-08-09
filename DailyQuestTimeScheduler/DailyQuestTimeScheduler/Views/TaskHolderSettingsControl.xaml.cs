using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for TaskHolderSettingsControl.xaml
    /// </summary>
    public partial class TaskHolderSettingsControl : UserControl, INotifyPropertyChanged
    {
        #region Members

        private TaskHolder taskHolder = null;
        private Func<TaskHolder, Task> onAcceptButtonClick;
        private Action onCancelButtonClick;
        public event PropertyChangedEventHandler PropertyChanged;

        private string title;
        private string description;
        private bool isRepeat = false;
        private byte weeklyRepeatPattern;
        private int taskDuration;
        private DateTime timeTakeToMakeTaskStart;
        private DateTime timeTakeToMakeTaskEnd;
        private DateTime dueDate = DateTime.Now;

        private bool sundayBool;
        private bool mondayBool;
        private bool tuesdayBool;
        private bool wednesdayBool;
        private bool thursdayBool;
        private bool fridayBool;
        private bool saturdayBool;


        #endregion

        #region Properties
        public Func<TaskHolder,Task> OnAcceptButtonClick
        {
            get { return onAcceptButtonClick; }
            set { onAcceptButtonClick = value; }
        }

        public Action OnCancelButtonClick
        {
            get { return onCancelButtonClick; }
            set { onCancelButtonClick = value; }
        }

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        public bool IsRepeat
        {
            get { return isRepeat; }
            set
            {
                isRepeat = value;
                OnPropertyChanged();
                OnPropertyChanged("IsNotRepeat");
            }
        }

        public bool IsNotRepeat
        {
            get { return !isRepeat; }
            set
            {
                isRepeat = !value;
            }
        }

        public byte WeeklyRepeatPattern
        {
            get { return weeklyRepeatPattern; }
            set
            {
                weeklyRepeatPattern = value;
                OnPropertyChanged();
            }
        }
        public int TaskDuration 
        {
            get { return taskDuration; }
            set
            {
                taskDuration = value;
                OnPropertyChanged();
            }
                
        }//how much day each task will takes

        public DateTime TimeTakeToMakeTaskStart
        {
            get { return timeTakeToMakeTaskStart; }
            set
            {
                timeTakeToMakeTaskStart = value;
                OnPropertyChanged();
            }
        }

        public DateTime TimeTakeToMakeTaskEnd
        {
            get { return timeTakeToMakeTaskEnd; }
            set
            {
                timeTakeToMakeTaskEnd = value;
                OnPropertyChanged();
            }
        }

        public DateTime DueDate
        {
            get { return dueDate; }
            set
            {
                dueDate = value;
                OnPropertyChanged();
            }

        }

        #region dayOfWeekProperties
        public bool SundayBool
        {
            get { return sundayBool; }
            set
            {
                sundayBool = value;
                OnPropertyChanged();
            }
        }

        public bool MondayBool
        {
            get { return mondayBool; }
            set
            {
                mondayBool = value;
                OnPropertyChanged();
            }
        }

        public bool TuesdayBool
        {
            get { return tuesdayBool; }
            set
            {
                tuesdayBool = value;
                OnPropertyChanged();
            }
        }

        public bool WednesdayBool
        {
            get { return wednesdayBool; }
            set
            {
                wednesdayBool = value;
                OnPropertyChanged();
            }
        }

        public bool ThursdayBool
        { 
            get { return thursdayBool; }
            set
            {
                thursdayBool = value;
                OnPropertyChanged();
            }
        }

        public bool FridayBool
        {
            get { return fridayBool; }
            set
            {
                fridayBool = value;
                OnPropertyChanged();
            }
        }

        public bool SaturdayBool
        {
            get { return saturdayBool; }
            set
            {
                saturdayBool = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #endregion

        #region Constructor
        public TaskHolderSettingsControl()
        {
            InitializeComponent();
            this.timeTakeToMakeTaskStart = DateTime.Now;
            FutureDatePicker.BlackoutDates.Add(new CalendarDateRange(new DateTime(0001, 1, 1), DateTime.Now.AddDays(-1)));
        }

        public TaskHolderSettingsControl(TaskHolder taskHolderRequiredSetting )
        {
            InitializeComponent();
            this.taskHolder = taskHolderRequiredSetting;
            this.Title = taskHolderRequiredSetting.Title;
            this.Description = taskHolderRequiredSetting.Description;
            this.IsRepeat = taskHolderRequiredSetting.IsRepeat;
            this.WeeklyRepeatPattern = taskHolderRequiredSetting.WeeklyRepeatPattern;
            this.TaskDuration = taskHolderRequiredSetting.TaskDuration;
            this.timeTakeToMakeTaskStart = DateTime.Now;
            this.ByteToDayOfWeekBool(this.weeklyRepeatPattern);
            FutureDatePicker.BlackoutDates.Add(new CalendarDateRange(new DateTime(0001, 1, 1), DateTime.Now.AddDays(-1)));
        }
        #endregion

        #region Method
        /// <summary>
        /// This method assigns the Week days bool properties from extracting byte.
        /// </summary>
        /// <param name="weeklyRepeatPattern"></param>
        private void ByteToDayOfWeekBool(byte weeklyRepeatPattern)
        {
            var weekOfDayboolList = new bool[7];
            for(int i = 0; i< 7; i++)
            {
                var checkingDay = (int)0b00000001 << i;

                if((this.WeeklyRepeatPattern & (checkingDay)) == checkingDay)
                {
                    weekOfDayboolList[i] = true;
                }
            }
            this.SundayBool = weekOfDayboolList[0];
            this.MondayBool = weekOfDayboolList[1];
            this.TuesdayBool = weekOfDayboolList[2];
            this.WednesdayBool = weekOfDayboolList[3];
            this.ThursdayBool = weekOfDayboolList[4];
            this.FridayBool = weekOfDayboolList[5];
            this.SaturdayBool = weekOfDayboolList[6];
        }
        /// <summary>
        /// return byte value made out of DayOfWeek properties
        /// </summary>
        /// <returns></returns>
        private byte GetWeeklyRepeatPattern()
        {
            int weeklyRepeatPattern = 0;
            if (SundayBool)
                weeklyRepeatPattern += 1;
            if (MondayBool)
                weeklyRepeatPattern += 2;
            if (TuesdayBool)
                weeklyRepeatPattern += 4;
            if (wednesdayBool)
                weeklyRepeatPattern += 8;
            if (thursdayBool)
                weeklyRepeatPattern += 16;
            if (fridayBool)
                weeklyRepeatPattern += 32;
            if (saturdayBool)
                weeklyRepeatPattern += 64;

            return (byte)weeklyRepeatPattern;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            OnCancelButtonClick?.Invoke();
        }

        /// <summary>
        /// Invoke the event with TaskHolder based on Properties that an user wrote.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            SetTaskHolderValues();

            OnAcceptButtonClick?.Invoke(taskHolder);
        }

        /// <summary>
        /// Set the TaskHolde based on Properties
        /// </summary>
        private void SetTaskHolderValues()
        {
            if (taskHolder == null) 
            {
                this.timeTakeToMakeTaskEnd = DateTime.Now;
                this.taskHolder = new NormalTaskHolder();

                this.taskHolder.TimeTakeToMakeTask = (int)((TimeTakeToMakeTaskEnd - TimeTakeToMakeTaskStart).TotalSeconds);
            }
            else
            {
                this.taskHolder.TimeTakeToMakeTask += (int)((TimeTakeToMakeTaskEnd - TimeTakeToMakeTaskStart).TotalSeconds);
            }

            this.taskHolder.Title = this.title;
            this.taskHolder.Description = this.description;
            this.taskHolder.IsRepeat = this.isRepeat;
            if (taskHolder is NormalTaskHolder normalTaskHolder)
            {
                if (isRepeat)
                    normalTaskHolder.TaskDuration = this.TaskDuration;
                else
                    normalTaskHolder.TaskDuration = (this.dueDate -DateTime.Now).Days;
            }
            this.taskHolder.WeeklyRepeatPattern = GetWeeklyRepeatPattern();
        }

        /// <summary>
        /// Rule: user can only write the numbers 0-9
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion
    }
}
