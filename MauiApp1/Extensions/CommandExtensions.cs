using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiApp1.Extensions
{
    public static class CommandExtensions
    {
        public static async Task ExecuteAsync(this ICommand command, object parameter = null)
        {
            if (command.CanExecute(parameter))
            {
                if (command is Command cmd)
                {
                    cmd.Execute(parameter);
                }
                else
                {
                    command.Execute(parameter);
                }
            }
            await Task.CompletedTask;
        }
    }
}