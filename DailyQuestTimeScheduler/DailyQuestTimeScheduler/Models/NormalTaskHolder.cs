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

        public NormalTaskHolder(DateTime initTime): base(initTime)
        {
        }

        public NormalTaskHolder(string displayTitle, string title, string description, bool isRepeat, byte weeklyRepeatPattern,
            int taskDuration, int timeTakeToMakeTask, string initTime) 
        {
            this.DisplayTitle = displayTitle;
            this.Title = title;
            this.Description = description;
            this.IsRepeat = isRepeat;
            this.WeeklyRepeatPattern = weeklyRepeatPattern;
            this.TaskDuration = taskDuration;
            this.TimeTakeToMakeTask = timeTakeToMakeTask;
            this.InitTime = initTime;
            this.CurrentTaskList = new List<UserTask>();
        }

        public NormalTaskHolder(string displayTitle, string title, string description, bool isRepeat, byte weeklyRepeatPattern,
            int taskDuration, int timeTakeToMakeTask, DateTime initTime)
        {
            this.DisplayTitle = displayTitle;
            this.Title = title;
            this.Description = description;
            this.IsRepeat = isRepeat;
            this.WeeklyRepeatPattern = weeklyRepeatPattern;
            this.TaskDuration = taskDuration;
            this.TimeTakeToMakeTask = timeTakeToMakeTask;
            this.initTime = initTime;
            this.CurrentTaskList = new List<UserTask>();

        }
    }
}
