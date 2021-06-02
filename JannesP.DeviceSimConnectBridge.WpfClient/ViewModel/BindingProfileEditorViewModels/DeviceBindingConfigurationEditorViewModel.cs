using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels
{
    public class BindingListViewModel : IBindingListViewModel
    {
        private ReadOnlyObservableCollection<IBindingEditorViewModel> _editors;

        public BindingListViewModel(string categoryName, ObservableCollection<IBindingEditorViewModel> editors)
        {
            CategoryName = categoryName;
            _editors = new ReadOnlyObservableCollection<IBindingEditorViewModel>(editors);
        }

        public override string CategoryName { get; }

        public override ReadOnlyObservableCollection<IBindingEditorViewModel> Editors
        {
            get => _editors;
            set
            {
                if (_editors != value)
                {
                    _editors = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public class DesignTimeBindingListViewModel : IBindingListViewModel
    {
        public DesignTimeBindingListViewModel(string categoryName, ObservableCollection<IBindingEditorViewModel> editors)
        {
            CategoryName = categoryName;
            Editors = new ReadOnlyObservableCollection<IBindingEditorViewModel>(editors);
        }

        public override string CategoryName { get; }
        public override ReadOnlyObservableCollection<IBindingEditorViewModel> Editors { get; set; }
    }

    public class DesignTimeDeviceBindingConfigurationEditorViewModel : IDeviceBindingConfigurationEditorViewModel
    {
        public override DeviceBindingConfiguration BindingConfig => throw new NotSupportedException();

        public override IEnumerable<IBindingListViewModel> BindingTypes { get; protected set; } = new List<IBindingListViewModel>()
        {
            new DesignTimeBindingListViewModel("Buttons", new ObservableCollection<IBindingEditorViewModel>()
            {
                new DesignTimeButtonBindingEditorViewModel(),
                new DesignTimeButtonBindingEditorViewModel(),
            }),
            new DesignTimeBindingListViewModel("Encoders", new ObservableCollection<IBindingEditorViewModel>()
            {
                new DesignTimeEncoderBindingEditorViewModel(),
                new DesignTimeEncoderBindingEditorViewModel(),
            }),
            new DesignTimeBindingListViewModel("Faders", new ObservableCollection<IBindingEditorViewModel>()
            {
                new DesignTimeFaderBindingEditorViewModel(),
                new DesignTimeFaderBindingEditorViewModel(),
            }),
            new DesignTimeBindingListViewModel("Leds", new ObservableCollection<IBindingEditorViewModel>()
            {
                new DesignTimeLedBindingEditorViewModel(),
                new DesignTimeLedBindingEditorViewModel(),
            }),
        };

        public override string? DeviceId { get; protected set; } = null;
        public override string DeviceType { get; protected set; } = "design_time_device_type";
        public override bool IsDeviceMissing { get; protected set; } = false;
        public override string Name { get; protected set; } = Guid.NewGuid().ToString();

        protected override void OnApplyChanges() => throw new NotSupportedException();

        protected override void OnRevertChanges() => throw new NotSupportedException();
    }

    public class DeviceBindingConfigurationEditorViewModel : IDeviceBindingConfigurationEditorViewModel
    {
        private readonly DeviceRepository _deviceRepository;
        private readonly IServiceProvider _serviceProvider;
        private IDevice? _device;
        private bool _isDeviceMissing;

        public DeviceBindingConfigurationEditorViewModel(IServiceProvider serviceProvider, DeviceBindingConfiguration bindingConfig, IDevice? device)
        {
            bindingConfig.ThrowIfNotComplete();
            _serviceProvider = serviceProvider;
            BindingConfig = bindingConfig;
            _device = device;
            Name = bindingConfig.FriendlyName;
            DeviceType = bindingConfig.DeviceType;
            DeviceId = bindingConfig.DeviceId;
            IsDeviceMissing = device == null;

            _deviceRepository = serviceProvider.GetRequiredService<DeviceRepository>();
            WeakEventManager<DeviceRepository, EventArgs>.AddHandler(_deviceRepository, nameof(DeviceRepository.DeviceListChanged), DeviceRepository_DeviceListChanged);

            CreateBindings();
            EnableTouchedTracking();
        }

        public override DeviceBindingConfiguration BindingConfig { get; }
        public override IEnumerable<IBindingListViewModel> BindingTypes { get; protected set; }

        public override string? DeviceId { get; protected set; }

        public override string DeviceType { get; protected set; }

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

        public override string Name { get; protected set; }

        protected override void OnApplyChanges()
        {
            //remove models that are now "empty"
            BindingConfig.Bindings.RemoveAll(bc => bc.IsEmpty);
            //add models that aren't empty and aren't already included
            BindingConfig.Bindings.AddRange(BindingTypes
                .SelectMany(bt => bt.Editors)
                .Select(b => b.Model)
                .Where(b => !b.IsEmpty && !BindingConfig.Bindings.Contains(b)));
        }

        protected override void OnRevertChanges()
        {
            /* nothing to do here */
        }

        [MemberNotNull(nameof(BindingTypes))]
        private void CreateBindings()
        {
            ObservableCollection<IBindingEditorViewModel> _buttonBindingViewModels;
            ObservableCollection<IBindingEditorViewModel> _encoderBindingViewModels;
            ObservableCollection<IBindingEditorViewModel> _ledBindingViewModels;
            if (_device != null)
            {
                IEnumerable<ButtonActionBinding> buttonBindings = BindingConfig.Bindings.OfType<ButtonActionBinding>();
                _buttonBindingViewModels = new ObservableCollection<IBindingEditorViewModel>(_device.Buttons
                    .Select(b => new ButtonBindingEditorViewModel(_serviceProvider, buttonBindings.SingleOrDefault(ab => ab.DeviceControlId == b.Id) ?? new ButtonActionBinding { DeviceControlId = b.Id }, b)));

                IEnumerable<EncoderActionBinding> encoderBindings = BindingConfig.Bindings.OfType<EncoderActionBinding>();
                _encoderBindingViewModels = new ObservableCollection<IBindingEditorViewModel>(_device.Encoders
                    .Select(e => new EncoderBindingEditorViewModel(_serviceProvider, encoderBindings.SingleOrDefault(ab => ab.DeviceControlId == e.Id) ?? new EncoderActionBinding { DeviceControlId = e.Id }, e)));

                /*
                var faders = new BindingListViewModel("Faders", device.Faders.Select(b => new FaderBindingEditorViewModel().ToList()));
                bindingTypes.Add(faders);*/

                IEnumerable<LedActionBinding> ledBindings = BindingConfig.Bindings.OfType<LedActionBinding>();
                _ledBindingViewModels = new ObservableCollection<IBindingEditorViewModel>(_device.Leds
                    .Select(led => new LedBindingEditorViewModel(_serviceProvider, ledBindings.SingleOrDefault(ab => ab.DeviceControlId == led.Id) ?? new LedActionBinding { DeviceControlId = led.Id }, led)));
            }
            else
            {
                _buttonBindingViewModels = new ObservableCollection<IBindingEditorViewModel>(BindingConfig.Bindings
                    .OfType<ButtonActionBinding>()
                    .Select(bab => new ButtonBindingEditorViewModel(_serviceProvider, bab, null)));

                _encoderBindingViewModels = new ObservableCollection<IBindingEditorViewModel>(BindingConfig.Bindings
                     .OfType<EncoderActionBinding>()
                     .Select(bab => new EncoderBindingEditorViewModel(_serviceProvider, bab, null)));

                /*
               var faders = new BindingListViewModel("Faders", device.Faders.Select(b => new FaderBindingEditorViewModel().ToList()));
               bindingTypes.Add(faders);*/

                _ledBindingViewModels = new ObservableCollection<IBindingEditorViewModel>(BindingConfig.Bindings
                    .OfType<LedActionBinding>()
                    .Select(bab => new LedBindingEditorViewModel(_serviceProvider, bab, null)));
            }

            IEnumerable<IBindingListViewModel>? bindingTypes = BindingTypes;
            if (bindingTypes == null)
            {
                bindingTypes = new List<IBindingListViewModel>()
                {
                    new BindingListViewModel("Buttons", _buttonBindingViewModels),
                    new BindingListViewModel("Encoders", _encoderBindingViewModels),
                    new BindingListViewModel("Leds", _ledBindingViewModels),
                };
            }
            else
            {
                RemoveChildren(bindingTypes.SelectMany(bl => bl.Editors));
                bindingTypes.Single(bt => bt.CategoryName == "Buttons").Editors = new ReadOnlyObservableCollection<IBindingEditorViewModel>(_buttonBindingViewModels);
                bindingTypes.Single(bt => bt.CategoryName == "Encoders").Editors = new ReadOnlyObservableCollection<IBindingEditorViewModel>(_encoderBindingViewModels);
                bindingTypes.Single(bt => bt.CategoryName == "Leds").Editors = new ReadOnlyObservableCollection<IBindingEditorViewModel>(_ledBindingViewModels);
            }
            AddChildren(bindingTypes.SelectMany(bl => bl.Editors));
            BindingTypes = bindingTypes;
        }

        private void DeviceRepository_DeviceListChanged(object? sender, EventArgs e)
        {
            if (_deviceRepository.TryFindDevice(DeviceType, DeviceId, out IDevice? foundDevice))
            {
                if (_device == null)
                {
                    _device = foundDevice;
                    CreateBindings();
                    IsDeviceMissing = false;
                }
            }
            else
            {
                if (_device != null) _device = null;
                IsDeviceMissing = true;
            }
        }
    }

    public abstract class IBindingListViewModel : ViewModelBase
    {
        public abstract string CategoryName { get; }
        public abstract ReadOnlyObservableCollection<IBindingEditorViewModel> Editors { get; set; }
    }

    public abstract class IDeviceBindingConfigurationEditorViewModel : RevertibleViewModelBase
    {
        public abstract DeviceBindingConfiguration BindingConfig { get; }
        public abstract IEnumerable<IBindingListViewModel> BindingTypes { get; protected set; }

        public abstract string? DeviceId { get; protected set; }

        public abstract string DeviceType { get; protected set; }

        /// <summary>
        /// If the device for the configuration could be found in the DeviceRepo.
        /// </summary>
        public abstract bool IsDeviceMissing { get; protected set; }

        public abstract string Name { get; protected set; }
    }
}