using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime
{
    public class DesignTimeCommand : ICommand
    {
        private readonly bool _canExecute;

        public DesignTimeCommand(bool canExecute = true)
        {
            _canExecute = canExecute;
        }

#pragma warning disable CS0067

        public event EventHandler? CanExecuteChanged;

#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => _canExecute;

        public void Execute(object? parameter) => throw new NotSupportedException();
    }
}