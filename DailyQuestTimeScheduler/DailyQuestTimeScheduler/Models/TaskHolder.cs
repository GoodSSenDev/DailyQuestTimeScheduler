using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
/// <summary>
/// 
/// This is abstract class that Holder current date of Task 
/// This also store a pattern of when each task popup and maybe way to save.
/// async and this Task holder have dependent to
/// </summary>
namespace DailyQuestTimeScheduler
{
    public abstract class TaskHolder
    {
        public UserTask CurrentDayTask { get; set; }
        //Title cannot change once it set
        private string title = null;
        public new string Title
        {
            get { return title; }
            set
            {
                if (title == null)
                {
                    title = value;
                }
            }
        }

        public string Description { get; set; }

        public bool IsRepeat { get; set; }

        public byte WeeklyRepeatPattern { get; set; }

        public int TaskDuration { get; set; }//how much day each task will takes

        public int TimeTakeToMakeTask { get; set; }

        public TaskHolder()
        {

        }


    }
}
