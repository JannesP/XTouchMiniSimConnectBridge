using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels
{
    public interface IBindingProfileEditorViewModel
    { 
        bool IsTouched { get; }
        ObservableCollection<IDeviceBindingConfigurationEditorViewModel> Devices { get; }
        void ApplyChanges();
    }

    public class DesignTimeBindingProfileEditorViewModel : IBindingProfileEditorViewModel
    {
        public ObservableCollection<IDeviceBindingConfigurationEditorViewModel> Devices { get; } = new ObservableCollection<IDeviceBindingConfigurationEditorViewModel>()
        {
            new DesignTimeDeviceBindingConfigurationEditorViewModel(),
            new DesignTimeDeviceBindingConfigurationEditorViewModel(),
            new DesignTimeDeviceBindingConfigurationEditorViewModel(),
        };

        public bool IsTouched => false;
        public void ApplyChanges() => throw new NotSupportedException();
    }

    public class BindingProfileEditorViewModel : ViewModelBase, IBindingProfileEditorViewModel
    {
        private readonly DeviceRepository _deviceRepository;
        private readonly BindingProfile _profile;
        private ObservableCollection<IDeviceBindingConfigurationEditorViewModel> _devices;

        public BindingProfileEditorViewModel(IServiceProvider service, BindingProfile profile)
        {
            _deviceRepository = service.GetRequiredService<DeviceRepository>();
            _profile = profile;

            IEnumerable<IDeviceBindingConfigurationEditorViewModel> deviceList = Enumerable.Empty<IDeviceBindingConfigurationEditorViewModel>();

            //add devices with configured bindings
            List<DeviceBindingConfigurationEditorViewModel>? inConfig = null;
            if (profile.BindingConfigurations != null)
            {
                inConfig = profile.BindingConfigurations.Select(bc => new DeviceBindingConfigurationEditorViewModel(service, bc)).ToList();
                deviceList = deviceList.Concat(inConfig);
            }
            //add devices that don't have any configured bindings
            IReadOnlyList<IDevice>? availableDeviceList = _deviceRepository.AvailableDevices;
            IEnumerable<IDevice> toAdd;
            if (inConfig == null)
            {
                toAdd = availableDeviceList;
            }
            else
            {
                toAdd = availableDeviceList.Where(ad => !inConfig.Any(ic => ic.DeviceType == ad.DeviceType && ic.DeviceId == ad.DeviceId));
            }
            deviceList = deviceList.Concat(toAdd.Select(device => new DeviceBindingConfigurationEditorViewModel(service, device)));


            _devices = new ObservableCollection<IDeviceBindingConfigurationEditorViewModel>(deviceList);
        }

        public ObservableCollection<IDeviceBindingConfigurationEditorViewModel> Devices
        {
            get => _devices; 
            private set
            {
                if (_devices != value)
                {
                    _devices = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTouched => false;

        public void ApplyChanges() => throw new NotImplementedException();
    }

}
