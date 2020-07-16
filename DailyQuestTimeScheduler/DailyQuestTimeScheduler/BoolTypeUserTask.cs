using System;
using System.Collections.Generic;
using System.Text;

namespace DailyQuestTimeScheduler
{
    /// <summary>
    /// 
    /// This is BoolType Task that the user can check whether they done the task or not.
    /// </summary>
    public class BoolTypeUserTask : UserTask
    {

        private bool isTaskDone;
        public bool IsTaskDone
        {
            get { return isTaskDone; }
            set
            {
                isTaskDone = value;
                if(value)
                {
                    this.SetCompletionTimeToNow();
                }
            }
        }
        public BoolTypeUserTask() : base()
        {

        }
        public BoolTypeUserTask(string title): base(title)
        {

        }

        public BoolTypeUserTask(string title, DateTime dateOfTask) : base(title, dateOfTask)
        {

        }
    }
}
