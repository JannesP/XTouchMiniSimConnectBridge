using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels
{
    public abstract class IDeviceBindingConfigurationEditorViewModel : ViewModelBase
    {
        /// <summary>
        /// If the device for the configuration could be found in the DeviceRepo.
        /// </summary>
        public abstract bool IsDeviceMissing { get; protected set; }
        public abstract string Name { get; protected set; }
        public abstract string DeviceType { get; protected set; }
        public abstract string? DeviceId { get; protected set; }
        public abstract IEnumerable<IBindingListViewModel> BindingTypes { get; protected set; }
    }

    public class DesignTimeDeviceBindingConfigurationEditorViewModel : IDeviceBindingConfigurationEditorViewModel
    {
        public override bool IsDeviceMissing { get; protected set; } = false;
        public override string Name { get; protected set; } = Guid.NewGuid().ToString();
        public override IEnumerable<IBindingListViewModel> BindingTypes { get; protected set; } = new List<IBindingListViewModel>() 
        {
            new DesignTimeBindingListViewModel("Buttons")
            {
                Editors = new List<DesignTimeButtonBindingEditorViewModel>()
                {
                    new DesignTimeButtonBindingEditorViewModel(),
                    new DesignTimeButtonBindingEditorViewModel(),
                }
            },
            new DesignTimeBindingListViewModel("Encoders")
            {
                Editors = new List<DesignTimeEncoderBindingEditorViewModel>()
                {
                    new DesignTimeEncoderBindingEditorViewModel(),
                    new DesignTimeEncoderBindingEditorViewModel(),
                }
            },
            new DesignTimeBindingListViewModel("Faders")
            {
                Editors = new List<DesignTimeFaderBindingEditorViewModel>()
                {
                    new DesignTimeFaderBindingEditorViewModel(),
                    new DesignTimeFaderBindingEditorViewModel(),
                }
            },
            new DesignTimeBindingListViewModel("Leds")
            {
                Editors = new List<DesignTimeLedBindingEditorViewModel>()
                {
                    new DesignTimeLedBindingEditorViewModel(),
                    new DesignTimeLedBindingEditorViewModel(),
                }
            }
        };

        public override string DeviceType { get; protected set; } = "design_time_device_type";
        public override string? DeviceId { get; protected set; } = null;
    }

    public class DeviceBindingConfigurationEditorViewModel : IDeviceBindingConfigurationEditorViewModel
    {
        private DeviceRepository _deviceRepository;
        private bool _isDeviceMissing;

        public DeviceBindingConfigurationEditorViewModel(IServiceProvider serviceProvider, DeviceBindingConfiguration bindingConfig)
        {
            bindingConfig.ThrowIfNotComplete();
            _deviceRepository = serviceProvider.GetRequiredService<DeviceRepository>();
            WeakEventManager<DeviceRepository, EventArgs>.AddHandler(_deviceRepository, nameof(DeviceRepository.DeviceListChanged), DeviceRepository_DeviceListChanged);
            if (_deviceRepository.TryFindDevice(bindingConfig.DeviceType, bindingConfig.DeviceId, out IDevice? device))
            {
                ConstructFromDevice(serviceProvider, device, bindingConfig);
            }
            else
            {
                Name = bindingConfig.FriendlyName;
                DeviceType = bindingConfig.DeviceType;
                DeviceId = bindingConfig.DeviceId;
                IsDeviceMissing = true;

                var bindingTypes = new List<IBindingListViewModel>();
                var buttons = new BindingListViewModel("Buttons", bindingConfig.Bindings
                    .OfType<ButtonActionBinding>()
                    .Select(bab => new ButtonBindingEditorViewModel(serviceProvider, null, bab))
                    .ToList());
                bindingTypes.Add(buttons);

               var encoders = new BindingListViewModel("Encoders", bindingConfig.Bindings
                    .OfType<EncoderActionBinding>()
                    .Select(bab => new EncoderBindingEditorViewModel(serviceProvider, null, bab))
                    .ToList());
                bindingTypes.Add(encoders);

                /*
               var faders = new BindingListViewModel("Faders", device.Faders.Select(b => new FaderBindingEditorViewModel().ToList()));
               bindingTypes.Add(faders);*/

                var leds = new BindingListViewModel("Leds", bindingConfig.Bindings
                    .OfType<LedActionBinding>()
                    .Select(bab => new LedBindingEditorViewModel(serviceProvider, null, bab))
                    .ToList());
                bindingTypes.Add(leds);

                BindingTypes = bindingTypes;
            }
        }

        public DeviceBindingConfigurationEditorViewModel(IServiceProvider serviceProvider, IDevice device)
        {
            ConstructFromDevice(serviceProvider, device);
            _deviceRepository = serviceProvider.GetRequiredService<DeviceRepository>();

        }

        private void DeviceRepository_DeviceListChanged(object? sender, EventArgs e)
        {
            if (!_deviceRepository.TryFindDevice(DeviceType, DeviceId, out IDevice? _))
            {
                IsDeviceMissing = true;
            }
        }

        [MemberNotNull(nameof(Name), nameof(DeviceType), nameof(BindingTypes), nameof(IsDeviceMissing))]
        private void ConstructFromDevice(IServiceProvider serviceProvider, IDevice device, DeviceBindingConfiguration? bindingConfig = null)
        {
            Name = device.FriendlyName;
            DeviceType = device.DeviceType;
            DeviceId = device.DeviceId;
            IsDeviceMissing = false;

            var bindingTypes = new List<IBindingListViewModel>();
            var buttons = new BindingListViewModel("Buttons", device.Buttons
                .Select(b => new ButtonBindingEditorViewModel(serviceProvider, b, bindingConfig?.Bindings.OfType<ButtonActionBinding>().FirstOrDefault(ab => ab.DeviceControlId == b.Id)))
                    .ToList());
            bindingTypes.Add(buttons);

            var encoders = new BindingListViewModel("Encoders", device.Encoders
                .Select(e => new EncoderBindingEditorViewModel(serviceProvider, e, bindingConfig?.Bindings.OfType<EncoderActionBinding>().FirstOrDefault(ab => ab.DeviceControlId == e.Id)))
                .ToList());
            bindingTypes.Add(encoders);

            /*
            var faders = new BindingListViewModel("Faders", device.Faders.Select(b => new FaderBindingEditorViewModel().ToList()));
            bindingTypes.Add(faders);*/

            var leds = new BindingListViewModel("Leds", device.Leds
                .Select(e => new LedBindingEditorViewModel(serviceProvider, e, bindingConfig?.Bindings.OfType<LedActionBinding>().FirstOrDefault(ab => ab.DeviceControlId == e.Id)))
                .ToList());
            bindingTypes.Add(leds);

            BindingTypes = bindingTypes;
        }

        public override string Name { get; protected set; }
        public override string DeviceType { get; protected set; }
        public override string? DeviceId { get; protected set; }
        public override IEnumerable<IBindingListViewModel> BindingTypes { get; protected set; }

        public override bool IsDeviceMissing
        {
            get => _isDeviceMissing;
            protected set
            {
                if (_isDeviceMissing != value)
                {
                    _isDeviceMissing = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public abstract class IBindingListViewModel : ViewModelBase
    {
        public abstract string CategoryName { get; }
        public abstract IEnumerable<IBindingEditorViewModel> Editors { get; set; }
    }

    public class BindingListViewModel : IBindingListViewModel
    {
        public BindingListViewModel(string categoryName, IEnumerable<IBindingEditorViewModel> editors)
        {
            CategoryName = categoryName;
            Editors = editors;
        }

        public override string CategoryName { get; }
        public override IEnumerable<IBindingEditorViewModel> Editors { get; set; }
    }



    public class DesignTimeBindingListViewModel : IBindingListViewModel
    {
        public override string CategoryName { get; }

        public DesignTimeBindingListViewModel(string categoryName)
        {
            CategoryName = categoryName;
            Editors = new List<IBindingEditorViewModel>();
        }

        public override IEnumerable<IBindingEditorViewModel> Editors { get; set; }
    }
}
