using System;
using System.Collections.Generic;
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
        public DateTime Date { get; set; }
        public DateTime TimeOfCompletionUTC { get; set; }
        public DateTime TimeOfCompletionLocal { get; set; }
        #endregion


        public UserTask(string titile = "NotSet")
        {
            this.Title = titile;
            this.Date = DateTime.Now;
        }

        public UserTask(string titile, DateTime dateOfTask)
        {
            this.Title = titile;
            this.Date = dateOfTask;
        }
        
        public void SetCompletionTimeToNow()
        {
            TimeOfCompletionUTC = DateTime.UtcNow;
            TimeOfCompletionLocal = DateTime.Now;
        }
    }
}
