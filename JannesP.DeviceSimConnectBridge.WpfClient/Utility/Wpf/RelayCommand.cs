﻿using System;
using System.Windows.Input;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _action;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> action)
        {
            _action = action;
        }

        public RelayCommand(Action<object?> action, Predicate<object?> canExecute) : this(action)
        {
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public static RelayCommand Empty { get; } = new RelayCommand(o => { });

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _action.Invoke(parameter);

        public void FireCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}