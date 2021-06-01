using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels
{
    public interface IBindingProfileEditorViewModel
    {
        ICommand CommandApplyChanges { get; }
        ICommand CommandRevertChanges { get; }
        ObservableCollection<IDeviceBindingConfigurationEditorViewModel> Devices { get; }
        bool IsTouched { get; }
    }

    public class BindingProfileEditorViewModel : RevertibleViewModelBase, IBindingProfileEditorViewModel
    {
        private readonly DeviceRepository _deviceRepository;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "I want this here, because it's the model.")]
        private readonly BindingProfile _profile;

        public BindingProfileEditorViewModel(IServiceProvider service, BindingProfile profile)
        {
            _deviceRepository = service.GetRequiredService<DeviceRepository>();
            _profile = profile;

            var deviceList = new List<IDeviceBindingConfigurationEditorViewModel>();
            //add all available devices
            IReadOnlyList<IDevice>? availableDeviceList = _deviceRepository.AvailableDevices;
            foreach (IDevice dev in availableDeviceList)
            {
                DeviceBindingConfiguration? config = profile.BindingConfigurations.FirstOrDefault(c => c.DeviceId == dev.DeviceId && c.DeviceType == dev.DeviceType);
                if (config == null)
                {
                    config = new DeviceBindingConfiguration
                    {
                        DeviceId = dev.DeviceId,
                        DeviceType = dev.DeviceType,
                        FriendlyName = dev.FriendlyName,
                    };
                }
                deviceList.Add(new DeviceBindingConfigurationEditorViewModel(service, config, dev));
            }
            //add devices that are configured but aren't available
            IEnumerable<DeviceBindingConfiguration> deviceMissing = profile.BindingConfigurations.Where(bc => !deviceList.Any(d => d.DeviceId == bc.DeviceId && d.DeviceType == bc.DeviceType));
            foreach (DeviceBindingConfiguration config in deviceMissing)
            {
                deviceList.Add(new DeviceBindingConfigurationEditorViewModel(service, config, null));
            }

            SetupCommands();

            Devices = new ObservableCollection<IDeviceBindingConfigurationEditorViewModel>(deviceList);
            Devices.CollectionChanged += Devices_CollectionChanged;
            AddChildren(Devices);
            EnableTouchedTracking();
        }

        public ICommand CommandApplyChanges { get; private set; }
        public ICommand CommandRevertChanges { get; private set; }
        public ObservableCollection<IDeviceBindingConfigurationEditorViewModel> Devices { get; }

        protected override void OnApplyChanges()
        {
            /* nothing to do here :) */
        }

        protected override void OnRevertChanges()
        {
            /* nothing to do here :) */
        }

        private void Devices_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null) AddChildren(e.NewItems.Cast<RevertibleViewModelBase>());
            if (e.OldItems != null) RemoveChildren(e.OldItems.Cast<RevertibleViewModelBase>());
        }

        [MemberNotNull(nameof(CommandApplyChanges), nameof(CommandRevertChanges))]
        private void SetupCommands()
        {
            CommandApplyChanges = new NotifiedRelayCommand(o => ApplyChanges(), o => IsTouched, this, nameof(IsTouched));
            CommandRevertChanges = new NotifiedRelayCommand(o => RevertChanges(), o => IsTouched, this, nameof(IsTouched));
        }
    }

    public class DesignTimeBindingProfileEditorViewModel : DesignTimeViewModel, IBindingProfileEditorViewModel
    {
        public ICommand CommandApplyChanges => EmptyCommand;

        public ICommand CommandRevertChanges => EmptyCommand;

        public ObservableCollection<IDeviceBindingConfigurationEditorViewModel> Devices { get; } = new ObservableCollection<IDeviceBindingConfigurationEditorViewModel>()
        {
            new DesignTimeDeviceBindingConfigurationEditorViewModel(),
            new DesignTimeDeviceBindingConfigurationEditorViewModel(),
            new DesignTimeDeviceBindingConfigurationEditorViewModel(),
        };

        public bool IsTouched => false;

        public void ApplyChanges() => throw new NotSupportedException();

        public void DiscardChanges() => throw new NotSupportedException();
    }
}