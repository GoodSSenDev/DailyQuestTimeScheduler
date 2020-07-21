using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace DailyQuestTimeScheduler.ViewModels
{
    public class MainWindowViewModel
    {
        public SqliteDataAccess DBAccess { get; set; }

        private ObservableCollection<BoolTypeUserTask> boolTypeTaskList = new ObservableCollection<BoolTypeUserTask>();
        public ObservableCollection<BoolTypeUserTask> BoolTypeTaskList
        {
            get { return boolTypeTaskList; }
        }

        public List<NormalTaskHolder> TaskHolderList { get; set; }

        public MainWindowViewModel()
        {
            this.DBAccess = new SqliteDataAccessSqliteCon();
        }

        public MainWindowViewModel(SqliteDataAccess dBAccess)
        {
            this.DBAccess = dBAccess;
        }

        public async Task<List<NormalTaskHolder>> GetTaskHolderListAsync()
        {
            return TaskHolderList = await DBAccess.GetTaskHolderListAsync();
        }

        // update the task that is checked using command 

    }
}
