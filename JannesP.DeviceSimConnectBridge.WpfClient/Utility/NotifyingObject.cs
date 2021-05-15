using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Utility
{
    public class NotifyingObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}