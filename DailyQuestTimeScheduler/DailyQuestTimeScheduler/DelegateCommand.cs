using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
/// <summary>
/// This class is implementation of DelegateCommand
/// </summary>
namespace DailyQuestTimeScheduler
{
    public class DelegateCommand : ICommand
    {
        readonly Action<object> executeMethod;
        readonly Func<object, bool> canExecuteMethod;

        public DelegateCommand(Action<object> executeMethod, Func<object, bool> canexecuteMethod)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canexecuteMethod;
        }

        public DelegateCommand(Action<object> execute) : this(execute, null)
        {

        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return (canExecuteMethod == null) ? true : canExecuteMethod(parameter);


        }

        public void Execute(object parameter)
        {
            executeMethod.Invoke(parameter);
        }
    }
}
