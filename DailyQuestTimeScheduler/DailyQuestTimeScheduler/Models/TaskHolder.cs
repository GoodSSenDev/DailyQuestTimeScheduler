using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Runtime.CompilerServices;
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
        #region members
        //Title cannot change once it set
        private string title = null;

        protected DateTime initTime;
        #endregion

        #region properties
        public List<UserTask> CurrentTaskList { get; set; }

        public string Title
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

        public string InitTime
        {
            get
            {
                return initTime.ToString("G", CultureInfo.CreateSpecificCulture("es-ES"));
            }
            set
            {
                initTime = Convert.ToDateTime(value, CultureInfo.CreateSpecificCulture("es-ES"));
            }
        }

        public DateTime InitTimeData
        {
            get { return initTime; }
        }

        #endregion

        //For DataBase (Default constructor)
        public TaskHolder()
        {
            this.CurrentTaskList = new List<UserTask>();
            this.initTime = DateTime.Now;
        }

        public TaskHolder(DateTime initTime)
        {
            this.initTime = initTime;
            this.CurrentTaskList = new List<UserTask>();
        }


    }
}
