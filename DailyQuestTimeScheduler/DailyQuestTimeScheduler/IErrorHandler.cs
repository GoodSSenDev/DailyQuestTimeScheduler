using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// This code and implementation was copied from John Thiriet https://github.com/johnthiriet 
/// (Thank You)
/// To avoid the using async void 
/// </summary>
namespace DailyQuestTimeScheduler
{
    public interface IErrorHandler
    {
        void HandleError(Exception EX);
    }
}
