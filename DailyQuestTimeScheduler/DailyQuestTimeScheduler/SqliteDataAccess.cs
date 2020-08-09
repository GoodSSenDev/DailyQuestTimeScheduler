using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// This is abstraction of Sqlite Data Access class.
/// </summary>
namespace DailyQuestTimeScheduler
{
    public abstract class SqliteDataAccess
    {
        //get all the list in the Task holder table in DB
        public abstract Task<List<NormalTaskHolder>> GetTaskHolderListAsync();

        public abstract Task<List<BoolTypeUserTask>> GetBoolTypeUserListAsync(string title);

        public abstract Task CreateNewTaskHolderAsync(TaskHolder taskHolder);

        public abstract Task DeleteTaskHolderAsync(string title);

        public abstract Task UpdateTaskHolderAsync(TaskHolder taskHolder);

        public abstract Task UpdateUserTaskAsync(UserTask userTask);

        public abstract Task InsertUserTaskAsync(UserTask userTask);

        public abstract Task UpsertUserTaskAsync(UserTask userTask);

        public abstract Task<BoolTypeUserTask> GetTaskOnSpecificDateAsync(string title, string date);
    }
}
