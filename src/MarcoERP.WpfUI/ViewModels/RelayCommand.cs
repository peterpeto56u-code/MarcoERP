using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MarcoERP.WpfUI.ViewModels
{
    /// <summary>
    /// Synchronous relay command for MVVM binding.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);

        /// <summary>Forces re-evaluation of CanExecute.</summary>
        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Async relay command for MVVM binding with async operations.
    /// Prevents double-execution while running.
    /// </summary>
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Func<object, bool> _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
            : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

        public async void Execute(object parameter)
        {
            if (_isExecuting) return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute(parameter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AsyncRelayCommand] Unhandled exception: {ex}");
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
