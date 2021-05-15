using System;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Options
{
    public class ApplicationOptions : NotifyingObject
    {
        private Guid? _currentProfileUniqueId = null;
        private string _simConnectApplicationName = AssemblyUtil.AssemblyNameWithoutExtension;
        private int _simConnectConnectRetryDelay = 10000;

        private string _singleInstanceMutexName = "JannesP_XTouchMiniSimConnectBridge";

        private bool _singleInstanceNotifyFirstInstance = true;

        public Guid? CurrentProfileUniqueId
        {
            get => _currentProfileUniqueId;
            set
            {
                if (_currentProfileUniqueId != value)
                {
                    _currentProfileUniqueId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SimConnectApplicationName
        {
            get => _simConnectApplicationName;
            set
            {
                if (_simConnectApplicationName != value)
                {
                    _simConnectApplicationName = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SimConnectConnectRetryDelay
        {
            get => _simConnectConnectRetryDelay;
            set
            {
                if (_simConnectConnectRetryDelay != value)
                {
                    _simConnectConnectRetryDelay = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SingleInstanceMutexName
        {
            get => _singleInstanceMutexName;
            set
            {
                if (_singleInstanceMutexName != value)
                {
                    _singleInstanceMutexName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool SingleInstanceNotifyFirstInstance
        {
            get => _singleInstanceNotifyFirstInstance;
            set
            {
                if (_singleInstanceNotifyFirstInstance != value)
                {
                    _singleInstanceNotifyFirstInstance = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}