using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DailyQuestTimeScheduler.Views
{
    /// <summary>
    /// Interaction logic for TaskHolderSettingsControl.xaml
    /// </summary>
    public partial class TaskHolderSettingsControl : UserControl, INotifyPropertyChanged
    {
        #region members
        private string title = null;
        private string description;
        private bool isRepeat;
        private byte weeklyRepeatPattern;
        private int taskDuration;
        private int timeTakeToMakeTask;

        #endregion

        #region properties

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
            }
        }

        public bool IsRepeat
        {
            get { return isRepeat; }
            set
            {
                isRepeat = value;
            }
        }

        public byte WeeklyRepeatPattern
        {
            get { return weeklyRepeatPattern; }
            set
            {
                weeklyRepeatPattern = value;
            }
        }
        public int TaskDuration 
        {
            get { return taskDuration; }
            set
            {
                taskDuration = value;
            }
                
        }//how much day each task will takes

        public int TimeTakeToMakeTask
        {
            get { return timeTakeToMakeTask; }
            set
            {
                timeTakeToMakeTask = value;
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public TaskHolderSettingsControl()
        {
            InitializeComponent();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
