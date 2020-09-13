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
            await Task.WhenAll(new List<Task>() { CreateTaskHolderRowAsync(taskHolder), CreateBoolTypeUserTaskTableAsync(taskHolder)});
        }

        private async Task CreateTaskHolderRowAsync(TaskHolder taskHolder)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {

                await cnn.ExecuteAsync("INSERT INTO TaskHolder (DisplayTitle, Title, IsRepeat, WeeklyRepeatPattern," +
                    " TaskDuration, TimeTakeToMakeTask, Description, InitTime) VALUES (@DisplayTitle, @Title, @IsRepeat, @WeeklyRepeatPattern, @TaskDuration, @TimeTakeToMakeTask," +
                    " @Description, @InitTime)", taskHolder);
            }
        }

        private async Task CreateBoolTypeUserTaskTableAsync(TaskHolder taskHolder)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync(@$"CREATE TABLE {taskHolder.Title} (Id INTEGER NOT NULL UNIQUE, IsTaskDone INTEGER, Date TEXT NOT NULL UNIQUE" +
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

        public override async Task UpdateTaskHolderAsync(TaskHolder taskHolder)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync(@"UPDATE TaskHolder 
                    SET DisplayTitle = @DisplayTitle, IsRepeat = @IsRepeat, WeeklyRepeatPattern = @WeeklyRepeatPattern, TaskDuration = @TaskDuration, 
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
                    await cnn.ExecuteAsync(@$"INSERT INTO {boolUserTask.ParentTaskHolder.Title} (IsTaskDone, Date," +
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
                    await cnn.ExecuteAsync(@$"UPDATE {boolUserTask.ParentTaskHolder.Title}
                    SET IsTaskDone = @IsTaskDone, TimeOfCompletionUTC = @TimeOfCompletionUTC, 
                        TimeOfCompletionLocal = @TimeOfCompletionLocal
                    WHERE Date = @Date;" 
                    , userTask);
                }
            }
        }

        public override async Task UpsertUserTaskAsync(UserTask userTask)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                if (userTask is BoolTypeUserTask boolUserTask)
                {
                    await cnn.ExecuteAsync(@$"INSERT INTO {boolUserTask.ParentTaskHolder.Title} (IsTaskDone, Date," +
                        " TimeOfCompletionUTC, TimeOfCompletionLocal) VALUES (" +
                        "@IsTaskDone, @Date, @TimeOfCompletionUTC, @TimeOfCompletionLocal)" +
                        "ON CONFLICT(Date) DO UPDATE SET " +
                        "IsTaskDone = @IsTaskDone, TimeOfCompletionUTC = @TimeOfCompletionUTC," +
                        " TimeOfCompletionLocal = @TimeOfCompletionLocal " +
                        "WHERE Date = @Date", userTask);
                }
            }
        }

        private string LoadConnectionString(string id = "Default")
        {
            //return configurationmanager.connectionstrings[id].connectionstring;
            return "Data Source=.\\TaskSaveData.db;Version=3;";
        }

        /// <summary>
        /// Method that allows find userTask on specific date.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="date"></param>
        /// <returns>Return first Element that found in database table and return NULL if not found anything</returns>
        public override async Task<BoolTypeUserTask> GetTaskOnSpecificDateAsync(string title, string date)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                string[] splitDateString = date.Split(' ');
                var output = await cnn.QueryAsync<BoolTypeUserTask>(@$"SELECT IsTaskDone, Date, TimeOfCompletionUTC,
                    TimeOfCompletionLocal FROM {title} Where Date LIKE @Date",new { Date = splitDateString[0]+"%" });

                var OutputList = output.ToList();

                if (OutputList.Count == 0)
                    return null;
                else return OutputList[0];
            }
        }
    }
}
