using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
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
        
        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsRepeat { get; set; }

        public byte RepeatDaysOfTheWeek { get; set; }
    }
}
