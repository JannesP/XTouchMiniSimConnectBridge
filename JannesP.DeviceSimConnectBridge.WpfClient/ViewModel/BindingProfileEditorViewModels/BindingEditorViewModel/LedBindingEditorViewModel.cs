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
        protected ILedBindingEditorViewModel(LedActionBinding actionBinding, IDeviceLed? deviceControl) : base(actionBinding, deviceControl)
        { }

        public abstract IEnumerable<ISimBoolSourceActionViewModel> AvailableDataSources { get; }
        public abstract ISimBoolSourceActionViewModel? DataSource { get; }
        public abstract ISimBoolSourceActionViewModel? SelectedDataSource { get; set; }
    }

    public class DesignTimeLedBindingEditorViewModel : ILedBindingEditorViewModel
    {
        private static int _instanceCount = 0;

        public DesignTimeLedBindingEditorViewModel() : base(null!, null)
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

        protected override void OnApplyChanges() => throw new NotSupportedException();
        protected override void OnRevertChanges() => throw new NotSupportedException();
    }

    public class LedBindingEditorViewModel : ILedBindingEditorViewModel
    {
        private readonly BindingActionRepository _bindingActionRepository;
        private readonly LedActionBinding _actionBinding;

        private ISimBoolSourceActionViewModel? _dataSource;

        public LedBindingEditorViewModel(IServiceProvider serviceProvider, LedActionBinding actionBinding, IDeviceLed? deviceLed) : base(actionBinding, deviceLed)
        {
            _bindingActionRepository = serviceProvider.GetRequiredService<BindingActionRepository>();
            _actionBinding = actionBinding;
            AvailableDataSources = _bindingActionRepository.GetAll<ISimBoolSourceAction>().Select(a => new SimBoolSourceActionViewModel(a)).ToList();
            LoadFromModel();
        }

        private void LoadFromModel()
        {
            if (_actionBinding.DataSource != null)
            {
                _dataSource = new SimBoolSourceActionViewModel(_actionBinding.DataSource);
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
                    if (_dataSource != null) RemoveChildren(_dataSource);
                    _dataSource = value?.CreateNew();
                    if (_dataSource != null) AddChildren(_dataSource);
                    OnPropertyChanged(true);
                    OnPropertyChanged(nameof(DataSource));
                }
            }
        }

        public override ISimBoolSourceActionViewModel? DataSource => _dataSource;

        protected override void OnApplyChanges()
        {
            _actionBinding.DataSource = DataSource?.Model;
        }

        protected override void OnRevertChanges()
        {
            LoadFromModel();
        }
    }
}
