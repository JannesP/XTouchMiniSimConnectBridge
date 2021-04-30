using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Options
{
    public class ApplicationOptions : NotifyingObject
    {
        private int _simConnectConnectRetryDelay = 10000;
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

        private string _singleInstanceMutexName = "JannesP_XTouchMiniSimConnectBridge";
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

        private string _simConnectApplicationName = AssemblyUtil.AssemblyNameWithoutExtension;
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

        private bool _singleInstanceNotifyFirstInstance = true;
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

        private Guid? _currentProfileUniqueId = null;
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
    }
}
