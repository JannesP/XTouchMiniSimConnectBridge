using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf
{
    public class NotifiedRelayCommand : RelayCommand
    {
        private readonly string[] _fireOnProperty;
        private readonly INotifyPropertyChanged _notifySource;

        public NotifiedRelayCommand(Action<object?> action, Predicate<object?> canExecute, INotifyPropertyChanged notifySource, params string[] fireOnProperty) : base(action, canExecute)
        {
            _notifySource = notifySource;
            _fireOnProperty = fireOnProperty;

            _notifySource.PropertyChanged += NotifySource_PropertyChanged;
        }

        private void NotifySource_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_fireOnProperty.Length == 0 || _fireOnProperty.Contains(e.PropertyName)) FireCanExecuteChanged();
        }
    }
}