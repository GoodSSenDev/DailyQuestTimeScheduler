using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

/// <summary>
/// This is abstract class for Task that the Users' daily or certain day task to hold the Users' data 
/// this the task should inherit this interface to be child of task holder.
/// 
/// </summay>

namespace DailyQuestTimeScheduler
{
    public abstract class UserTask
    {

        #region properties
        public string Title { get; set; }

        protected DateTime date;
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

        protected DateTime timeOfCompletionUTC;
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

        protected DateTime timeOfCompletionLocal { get; set; }
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
        #endregion

        public UserTask()
        {
            this.Title = "NotSet";
            this.date = DateTime.Now;
        }

        public UserTask(string titile = "NotSet")
        {
            this.Title = titile;
            this.date = DateTime.Now;
        }

        public UserTask(string titile, DateTime dateOfTask)
        {
            this.Title = titile;
            this.date = dateOfTask;
        }
        
        public void SetCompletionTimeToNow()
        {
            timeOfCompletionUTC = DateTime.UtcNow;
            timeOfCompletionLocal = DateTime.Now;
        }
    }
}
