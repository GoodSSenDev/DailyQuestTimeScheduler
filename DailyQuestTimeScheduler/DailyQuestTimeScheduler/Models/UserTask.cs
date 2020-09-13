using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// This is abstract class for Task that the Users' daily or certain day task to hold the Users' data 
/// this the task should inherit this interface to be child of task holder.
/// 
/// </summay>

namespace DailyQuestTimeScheduler
{
    public abstract class UserTask : INotifyPropertyChanged
    {
        #region member
        public event PropertyChangedEventHandler PropertyChanged;

        protected DateTime date;

        protected DateTime timeOfCompletionUTC;

        protected DateTime timeOfCompletionLocal;

        #endregion

        #region properties
        public string DisplayTitle { get; set; }

        public TaskHolder ParentTaskHolder { get; set; }

        public string Date
        {
            get
            {
                return date.ToString("G",
                  CultureInfo.CreateSpecificCulture("es-ES"));
            }
            set
            {
                date = Convert.ToDateTime(value, CultureInfo.CreateSpecificCulture("es-ES"));
            }
        }

        public DateTime DateData
        { 
            get { return date; }
        }

        public string TimeOfCompletionUTC
        {
            get { return timeOfCompletionUTC.ToString("G",
                    CultureInfo.CreateSpecificCulture("es-ES")); }
            set
            {
                timeOfCompletionUTC = Convert.ToDateTime(value, CultureInfo.CreateSpecificCulture("es-ES"));
            }
        }

        public DateTime TimeOfCompletionUTCData
        {
            get { return timeOfCompletionUTC; }
        }


        public string TimeOfCompletionLocal
        {
            get
            {
                return timeOfCompletionLocal.ToString("G",
                  CultureInfo.CreateSpecificCulture("es-ES"));
            }
            set
            {
                timeOfCompletionLocal = Convert.ToDateTime(value, CultureInfo.CreateSpecificCulture("es-ES"));
            }
        }
        public DateTime TimeOfCompletionLocalData
        {
            get { return timeOfCompletionLocal; }
        }

        /// <summary>
        /// This Delegate occurs when data change Invoke passes this class as parameter
        /// </summary>
        public Action<UserTask> OnDataChanged { get; set; }

        #endregion

        public UserTask()
        {
            this.DisplayTitle = "NotSet";
            this.date = DateTime.Now;
        }

        public UserTask(string displayTitle)
        {
            this.DisplayTitle = displayTitle;
            this.date = DateTime.Now;
        }

        public UserTask(string displayTitle, DateTime dateOfTask)
        {
            this.DisplayTitle = displayTitle;
            this.date = dateOfTask;
        }
        
        public void SetCompletionTimeToNow()
        {
            timeOfCompletionUTC = DateTime.UtcNow;
            timeOfCompletionLocal = DateTime.Now;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void ActionOnDataChanged()
        {
            OnDataChanged?.Invoke(this);
        }
    }
}

