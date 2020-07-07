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
        DateTime Date { get; set; }
        DateTime TimeOfCompletionUTC { get; set; }
        DateTime TimeOfCompletionLocal { get; set; }
        #endregion

        public UserTask()
        {
            this.Date = DateTime.Now;
        }
        public UserTask(DateTime dateOfTask)
        {
            this.Date = dateOfTask;
        }

        
        public void SetCompletionTimeToNow()
        {
            TimeOfCompletionUTC = DateTime.UtcNow;
            TimeOfCompletionLocal = DateTime.Now;
        }
    }
}
