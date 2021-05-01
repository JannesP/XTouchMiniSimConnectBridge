using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf
{
    public class RelayCommand : ICommand
    {
        public static RelayCommand Empty { get; } = new RelayCommand(o => { });

        private readonly Action<object?> _action;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> action) : this(action, null) { }
        public RelayCommand(Action<object?> action, Predicate<object?>? canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public void FireCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _action.Invoke(parameter);
    }
}
