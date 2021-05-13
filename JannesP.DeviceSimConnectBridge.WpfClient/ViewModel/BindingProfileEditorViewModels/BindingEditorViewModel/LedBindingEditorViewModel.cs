using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.DataSourceViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public abstract class ILedBindingEditorViewModel : IBindingEditorViewModel
    {
        protected ILedBindingEditorViewModel(IDeviceLed? deviceControl) : base(deviceControl)
        { }

        public abstract IEnumerable<ISimBoolSourceActionViewModel> AvailableDataSources { get; }
        public abstract ISimBoolSourceActionViewModel? DataSource { get; }
        public abstract ISimBoolSourceActionViewModel? SelectedDataSource { get; set; }
    }

    public class DesignTimeLedBindingEditorViewModel : ILedBindingEditorViewModel
    {
        private static int _instanceCount = 0;

        public DesignTimeLedBindingEditorViewModel() : base(null)
        {
            DataSource = AvailableDataSources.Skip(2).First();
        }

        public override string Name { get; } = $"Design Time Led {++_instanceCount}";

        public override IEnumerable<ISimBoolSourceActionViewModel> AvailableDataSources { get; } = new List<ISimBoolSourceActionViewModel>
        {
            new DesignTimeSimBoolSourceActionViewModel(),
            new DesignTimeSimBoolSourceActionViewModel(),
            new DesignTimeSimBoolSourceActionViewModel(),
            new DesignTimeSimBoolSourceActionViewModel(),
        };

        public override ISimBoolSourceActionViewModel? DataSource { get; }

        public override ISimBoolSourceActionViewModel? SelectedDataSource { get => DataSource; set => throw new NotSupportedException(); }

        public override ActionBinding CreateModel() => throw new NotSupportedException();
    }

    public class LedBindingEditorViewModel : ILedBindingEditorViewModel
    {
        private readonly BindingActionRepository _bindingActionRepository;
        private readonly IDeviceLed? _deviceLed;
        private readonly ActionBinding? _actionBinding;

        private ISimBoolSourceActionViewModel? _dataSource;

        public LedBindingEditorViewModel(IServiceProvider serviceProvider, IDeviceLed? deviceLed, ActionBinding? actionBinding) : base(deviceLed)
        {
            _bindingActionRepository = serviceProvider.GetRequiredService<BindingActionRepository>();
            _deviceLed = deviceLed;
            _actionBinding = actionBinding;
            AvailableDataSources = _bindingActionRepository.GetAll<ISimBoolSourceAction>().Select(a => new SimBoolSourceActionViewModel(a)).ToList();
            if (actionBinding is LedActionBinding binding)
            {
                if (binding.DataSource != null)
                {
                    _dataSource = new SimBoolSourceActionViewModel(binding.DataSource);
                }
            }
        }

        public override IEnumerable<ISimBoolSourceActionViewModel> AvailableDataSources { get; }


        public override ISimBoolSourceActionViewModel? SelectedDataSource
        {
            get => AvailableDataSources.FirstOrDefault(a => a.UniqueIdentifier == _dataSource?.UniqueIdentifier);
            set
            {
                if (value?.GetType() != _dataSource?.GetType())
                {
                    _dataSource = value?.CreateNew();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DataSource));
                }
            }
        }

        public override ISimBoolSourceActionViewModel? DataSource => _dataSource;

        public override string Name => _deviceLed?.Name ?? "<Device Not Available>";

        public override ActionBinding CreateModel() => throw new NotImplementedException();
    }

    
}
