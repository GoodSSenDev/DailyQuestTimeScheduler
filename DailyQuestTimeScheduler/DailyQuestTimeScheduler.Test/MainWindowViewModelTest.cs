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
            var testTaskHolderExpect = new NormalTaskHolder("Test", "this is testing2", true, 0b01010101, 1, 3214);


            dBAccessClass.Setup(x => x.GetTaskHolderListAsync())
                .ReturnsAsync((new List<NormalTaskHolder>
                {  new NormalTaskHolder("Test", "this is testing2", true, 0b01010101, 1, 3214)}));

            mainWindowVM = new MainWindowViewModel(dBAccessClass.Object);

            var actual =  await mainWindowVM.GetTaskHolderListAsync();

            Assert.Equal(testTaskHolderExpect.Title, actual[0].Title);
            Assert.Equal(testTaskHolderExpect.WeeklyRepeatPattern, actual[0].WeeklyRepeatPattern);
            Assert.Equal(testTaskHolderExpect.Description, actual[0].Description);
        }


    }
}
