using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using DailyQuestTimeScheduler.ViewModels;
using Moq;
using Xunit;


namespace DailyQuestTimeScheduler.Tests
{
    public class MainWindowViewModelTest
    {
        MainWindowViewModel mainWindowVM;

        public MainWindowViewModelTest()
        {
            mainWindowVM = new MainWindowViewModel();
        }

        [Fact]
        public async Task GetTaskHolderList_ShouldGetTheTaskHolderDataFromDB()
        {
            var dBAccessClass = new Mock<SqliteDataAccess>();
            var testTaskHolderExpect = new NormalTaskHolder("Test", "this is testing2", true, 0b01010101, 1, 3214, DateTime.Now);


            dBAccessClass.Setup(x => x.GetTaskHolderListAsync())
                .ReturnsAsync((new List<NormalTaskHolder>
                {  new NormalTaskHolder("Test", "this is testing2", true, 0b01010101, 1, 3214,DateTime.Now)}));

            mainWindowVM = new MainWindowViewModel(dBAccessClass.Object);

            var actual =  await mainWindowVM.GetTaskHolderListAsync();

            Assert.Equal(testTaskHolderExpect.Title, actual[0].Title);
            Assert.Equal(testTaskHolderExpect.WeeklyRepeatPattern, actual[0].WeeklyRepeatPattern);
            Assert.Equal(testTaskHolderExpect.Description, actual[0].Description);
        }

        /// <summary>
        /// Test Can this BringUnfinishedTasks method brings Unfinshed task data from the DB or if not exsisting making one 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task BringUnfinishedTasks_ShouldReturnUnfinishedTasks()
        {
            byte Test1WeekPattern = 0b01010101;
            byte Test2WeekPattern = 0b00101010;

            var dBAccessClass = new Mock<SqliteDataAccess>();

            dBAccessClass.Setup(x => x.GetTaskOnSpecificDateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new BoolTypeUserTask() { IsTaskDone = true });

            this.mainWindowVM = new MainWindowViewModel(dBAccessClass.Object);

            mainWindowVM.TaskHolderList = new List<NormalTaskHolder>
            {
               new NormalTaskHolder("Test1", "this is testing1", true, Test1WeekPattern, 2, 3214,DateTime.Now + TimeSpan.FromDays(-10)),
               new NormalTaskHolder("Test2", "this is testing2", true, 0b00101010, 2, 3214,DateTime.Now + TimeSpan.FromDays(-10))
            };

            await mainWindowVM.BringUnfinishedBoolTypeTasksAsync();

            ////Since enum of Sunday from DayOfWeek property is 0 which, I want it to be highest number
            ////So I made Monday 1 to 0 and sunday as 0 to 6 
            ////So I can bit shift and campare to pattern
            //var dayOfWeekToday = ((int)DateTime.Now.DayOfWeek - 1) % 7;


            var Today = 0b00000001 << (int)DateTime.Now.DayOfWeek;

            if ((Test1WeekPattern & Today) == Today)
            {
                Assert.Single(mainWindowVM.TaskHolderList[0].CurrentTaskList);
            }
            if ((Test2WeekPattern & Today) == Today)
            {
                Assert.Single(mainWindowVM.TaskHolderList[1].CurrentTaskList);
            }

        }
    }
    
}
