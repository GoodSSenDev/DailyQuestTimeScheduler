using System;
using Xunit;
using DailyQuestTimeScheduler;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Globalization;
/// <summary>
/// This class test Database connection amd a data access functianality of DataAccessClass 
/// </summary>
namespace DailyQuestTimeScheduler.Tests
{
    [Collection("Sequential")]
    public class SqliteDataAccessSqliteConTest : IAsyncLifetime
    {
        SqliteDataAccess databaseAccess;
        TaskHolder taskHolder;

        public SqliteDataAccessSqliteConTest()
        {
            databaseAccess = new SqliteDataAccessSqliteCon();

            taskHolder = new NormalTaskHolder("Test", "this is testing", true, 0b01010101, 1, 32);
        }

        //Create a testing row and a testing table
        public async Task InitializeAsync()
        {
            if (databaseAccess is SqliteDataAccessSqliteCon databaseAccessSqlite)
            {
                await databaseAccessSqlite.CreateNewTaskHolderAsync(taskHolder);
            }
        }
        
        //Delete the testing row and the testing table 
        public async Task DisposeAsync()
        {
            if (databaseAccess is SqliteDataAccessSqliteCon databaseAccessSqlite)
            {
                await databaseAccessSqlite.DeleteTaskHolderAsync(taskHolder.Title);
            }
        }

        //check numOfRow for is the data is inserted or not 
        //check the first row's title to see is this data we added
        [Fact]
        public async void CreateTaskHolderRowAsync_ShouldAddedNewRowInTaskHolderTable()
        {
            var numOfRowInTaskHolderTable = -1;
            string titleOfFirstRow = "";
            byte repeatPatternOfFirstRow = 0b010101;

            if (databaseAccess is SqliteDataAccessSqliteCon databaseAccessSqlite)
            {

                var list = await databaseAccessSqlite.GetTaskHolderListAsync();
                numOfRowInTaskHolderTable = list.Count;
                titleOfFirstRow = list[0].Title;
                repeatPatternOfFirstRow = list[0].WeeklyRepeatPattern;
            }

            Assert.Equal(1, numOfRowInTaskHolderTable);
            Assert.Equal("Test", titleOfFirstRow);
            Assert.Equal(0b01010101, repeatPatternOfFirstRow);
        }

        /// <summary>
        /// This test is for testing Updating the taskholder table with usertask table 
        /// if CreateTaskHolderRowAsync_ShouldAddedNewRowInTaskHolderTable() test not works than 
        /// this test will not works.
        /// </summary>
        [Theory]
        [InlineData("thi1s is testing", true, 0b01010101, 1, 3214)]
        [InlineData("this is testi2ng", false, 0b00000001, 2, 323)]
        [InlineData("thi3s is testing", false, 0b01000101, 3, 213)]
        [InlineData("th4is is testing", true, 0b01010101, 4, 332)]
        public async void UpdateTaskHolderAsync_SettingShouldChanged(string description, bool isRepeat,
            byte weeklyRepeatPattern, int taskDuration, int timeTakeToMakeTask)
        {
            var testTaskHolder = new NormalTaskHolder("Test", description, isRepeat, weeklyRepeatPattern,
                taskDuration, timeTakeToMakeTask);


            string descriptionOfFirstRow = "";
            byte repeatPatternOfFirstRow = 0b00000;
            bool isRepeatOfFirstRow = false;
            int taskdurationOfFirstRow = 0;
            int timeTakeToMakeTaskOfFirstRow = 0;

            if (databaseAccess is SqliteDataAccessSqliteCon databaseAccessSqlite)
            {
                await databaseAccessSqlite.UpdateTaskHolderAsync(testTaskHolder);

                var list = await databaseAccessSqlite.GetTaskHolderListAsync();
                descriptionOfFirstRow = list[0].Description;
                repeatPatternOfFirstRow = list[0].WeeklyRepeatPattern;
                isRepeatOfFirstRow = list[0].IsRepeat;
                taskdurationOfFirstRow = list[0].TaskDuration;
                timeTakeToMakeTaskOfFirstRow = list[0].TimeTakeToMakeTask;
            }



            Assert.Equal(testTaskHolder.Description, descriptionOfFirstRow);
            Assert.Equal(testTaskHolder.WeeklyRepeatPattern, repeatPatternOfFirstRow);
            Assert.Equal(testTaskHolder.IsRepeat, isRepeatOfFirstRow);
            Assert.Equal(testTaskHolder.TaskDuration, taskdurationOfFirstRow);
            Assert.Equal(testTaskHolder.TimeTakeToMakeTask, timeTakeToMakeTaskOfFirstRow);
        }

        /// <summary>
        /// This Test will check insertion
        /// but if getlist of bool type user task method not working this test will also not works
        /// </summary>
        [Fact]
        public async void InsertUserTaskAsync_ShouldUpdatetheProperty()
        {

            List<BoolTypeUserTask> boolTypeUserList = new List<BoolTypeUserTask>();

            var userTask = new BoolTypeUserTask(taskHolder.Title);
            if (databaseAccess is SqliteDataAccessSqliteCon databaseAccessSqlite)
            {
                await databaseAccessSqlite.InsertUserTaskAsync(userTask);
                boolTypeUserList = await databaseAccessSqlite.GetBoolTypeUserListAsync(userTask.Title);

            }

            Assert.Equal(userTask.Date
                 , boolTypeUserList[0].Date);

        }

        /// <summary>
        /// Check inserted data gpt Updated if the insertion test not working than this test also not works
        /// </summary>
        [Fact]
        public async void UpdateUserTaskAsync_ShouldUpdatetheProperty()
        {

            bool IsTaskDoneAftereChange = false;

            List<BoolTypeUserTask> boolTypeUserList = new List<BoolTypeUserTask>();

            var userTask = new BoolTypeUserTask(taskHolder.Title);
            userTask.IsTaskDone = false;
            
            var secondUserTask = new BoolTypeUserTask(taskHolder.Title);
            secondUserTask.Date = userTask.Date;
            secondUserTask.IsTaskDone = true;

            if (databaseAccess is SqliteDataAccessSqliteCon databaseAccessSqlite)
            {
                await databaseAccessSqlite.InsertUserTaskAsync(userTask);
                
                await databaseAccessSqlite.UpdateUserTaskAsync(secondUserTask);
                boolTypeUserList = await databaseAccessSqlite.GetBoolTypeUserListAsync(userTask.Title);
                IsTaskDoneAftereChange = boolTypeUserList[0].IsTaskDone;
            }

            Assert.Single(boolTypeUserList);
            Assert.True(IsTaskDoneAftereChange);

        }
    }
} 
