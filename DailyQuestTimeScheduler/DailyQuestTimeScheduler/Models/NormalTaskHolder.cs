using System;
using System.Collections.Generic;
using System.Text;
/// <summary>
/// Model
/// Abstraction of normalTaskholder
/// </summary>
namespace DailyQuestTimeScheduler
{
    public class NormalTaskHolder : TaskHolder
    {

        public NormalTaskHolder()
        {
        }
            
        public NormalTaskHolder(string title, string description, bool isRepeat, byte weeklyRepeatPattern,
            int taskDuration, int timeTakeToMakeTask) 
        {
            this.Title = title;
            this.Description = description;
            this.IsRepeat = isRepeat;
            this.WeeklyRepeatPattern = weeklyRepeatPattern;
            this.TaskDuration = taskDuration;
            this.TimeTakeToMakeTask = timeTakeToMakeTask;
        }
    }
}
