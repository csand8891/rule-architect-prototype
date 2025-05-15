using System;
using System.Windows.Input;

// Suggested Namespace
namespace RuleArchitectPrototype.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute; // Changed to Action<object> for flexibility
        private readonly Predicate<object> _canExecute; // Changed to Predicate<object>

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Constructor for commands that don't take a parameter
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(param => execute(), param => canExecute == null || canExecute())
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
        }


        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}