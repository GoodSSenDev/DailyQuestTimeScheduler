using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace DailyQuestTimeScheduler
{
    public class SqliteDataAccessSqliteCon : SqliteDataAccess
    {

        public override async Task CreateNewTaskHolderAsync(TaskHolder taskHolder)
        {
            //for now just bool type userTask only so no need to do other thing. // pattern matching required.
            await Task.WhenAll(new List<Task>() { CreateTaskHolderRowAsync(taskHolder), CreateBoolTypeUserTaskTableAsync(taskHolder)});
        }

        private async Task CreateTaskHolderRowAsync(TaskHolder taskHolder)
        {

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {

                await cnn.ExecuteAsync("INSERT INTO TaskHolder (Title, IsRepeat, WeeklyRepeatPattern," +
                    " TaskDuration, TimeTakeToMakeTask, Description) VALUES (@Title, @IsRepeat, @WeeklyRepeatPattern, @TaskDuration, @TimeTakeToMakeTask," +
                    " @Description)", taskHolder);
            }
        }

        private async Task CreateBoolTypeUserTaskTableAsync(TaskHolder taskHolder)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync(@$"CREATE TABLE {taskHolder.Title} (Id INTEGER NOT NULL UNIQUE, IsTaskDone INTEGER, Date TEXT NOT NULL" +
                    $", TimeOfCompletionUTC TEXT, TimeOfCompletionLocal TEXT, PRIMARY KEY(Id AUTOINCREMENT))");
            }
        }

        public override async Task DeleteTaskHolderAsync(string title)
        {
            await Task.WhenAll(new List<Task>() { DeleteTaskHolderRowAsync(title), DeleteUserTaskTableAsync(title) });
        }

        private async Task DeleteTaskHolderRowAsync(string title)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync("DELETE FROM TaskHolder WHERE Title = @Title", new { Title = title });
            }
        }

        private async Task DeleteUserTaskTableAsync(string title)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"DROP TABLE IF EXISTS {title}");
            }
        }

        public override async Task<List<NormalTaskHolder>> GetTaskHolderListAsync()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<NormalTaskHolder>("SELECT * FROM TaskHolder", new DynamicParameters());
                return output.ToList();
            }
        }

        public override async Task UpdateTaskHolderAsync(TaskHolder taskHolder)// fix name Async
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync(@"UPDATE TaskHolder 
                    SET IsRepeat = @IsRepeat, WeeklyRepeatPattern = @WeeklyRepeatPattern, TaskDuration = @TaskDuration, 
                        TimeTakeToMakeTask = @TimeTakeToMakeTask, Description = @Description 
                    WHERE Title = @Title;"
                    , taskHolder);
            }
        }

        public override async Task<List<BoolTypeUserTask>> GetBoolTypeUserListAsync(string title)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<BoolTypeUserTask>(@$"SELECT IsTaskDone, Date, TimeOfCompletionUTC, TimeOfCompletionLocal FROM {title}", new DynamicParameters());
                return output.ToList();
            }
        }


        public override async Task InsertUserTaskAsync(UserTask userTask)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                //pattern matching required later
                if (userTask is BoolTypeUserTask boolUserTask)
                {
                    await cnn.ExecuteAsync(@$"INSERT INTO {boolUserTask.Title} (IsTaskDone, Date," +
                        " TimeOfCompletionUTC, TimeOfCompletionLocal) VALUES (" +
                        "@IsTaskDone, @Date, @TimeOfCompletionUTC, @TimeOfCompletionLocal)", userTask);
                }
            }
        }


        public override async Task UpdateUserTaskAsync(UserTask userTask)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                if (userTask is BoolTypeUserTask boolUserTask)
                {
                    await cnn.ExecuteAsync(@$"UPDATE {boolUserTask.Title}
                    SET IsTaskDone = @IsTaskDone, TimeOfCompletionUTC = @TimeOfCompletionUTC, 
                        TimeOfCompletionLocal = @TimeOfCompletionLocal
                    WHERE Date = @Date;"// need to fix this 
                    , userTask);
                }
            }
        }

        private string LoadConnectionString(string id = "Default")
        {
            //return configurationmanager.connectionstrings[id].connectionstring;
            return "Data Source=.\\TaskSaveData.db;Version=3;";
        }
    }
}
