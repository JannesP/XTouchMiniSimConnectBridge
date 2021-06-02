using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.DataSourceViewModels;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public class DesignTimeLedBindingEditorViewModel : ILedBindingEditorViewModel
    {
        private static int _instanceCount = 0;

        public DesignTimeLedBindingEditorViewModel() : base(null!, null)
        {
            DataSource = AvailableDataSources.Skip(2).First();
            ConfigurationSummary = DataSource.ConfigurationSummary;
        }

        public override IEnumerable<ISimBoolSourceActionViewModel> AvailableDataSources { get; } = new List<ISimBoolSourceActionViewModel>
        {
            new DesignTimeSimBoolSourceActionViewModel(),
            new DesignTimeSimBoolSourceActionViewModel(),
            new DesignTimeSimBoolSourceActionViewModel(),
            new DesignTimeSimBoolSourceActionViewModel(),
        };

        public override ICommand CommandClearDataSource { get; } = new DesignTimeCommand();
        public override string ConfigurationSummary { get; protected set; }
        public override ISimBoolSourceActionViewModel? DataSource { get; }
        public override string Name { get; } = $"Design Time Led {++_instanceCount}";
        public override ISimBoolSourceActionViewModel? SelectedDataSource { get => DataSource; set => throw new NotSupportedException(); }

        protected override void OnApplyChanges() => throw new NotSupportedException();

        protected override void OnRevertChanges() => throw new NotSupportedException();
    }

    public abstract class ILedBindingEditorViewModel : IBindingEditorViewModel
    {
        protected ILedBindingEditorViewModel(LedActionBinding actionBinding, IDeviceLed? deviceControl) : base(actionBinding, deviceControl)
        { }

        public abstract IEnumerable<ISimBoolSourceActionViewModel> AvailableDataSources { get; }
        public abstract ICommand CommandClearDataSource { get; }
        public abstract ISimBoolSourceActionViewModel? DataSource { get; }
        public abstract ISimBoolSourceActionViewModel? SelectedDataSource { get; set; }
    }

    public class LedBindingEditorViewModel : ILedBindingEditorViewModel
    {
        private readonly LedActionBinding _actionBinding;
        private readonly BindingActionRepository _bindingActionRepository;
        private ISimBoolSourceActionViewModel? _dataSource;

        public LedBindingEditorViewModel(IServiceProvider serviceProvider, LedActionBinding actionBinding, IDeviceLed? deviceLed) : base(actionBinding, deviceLed)
        {
            _bindingActionRepository = serviceProvider.GetRequiredService<BindingActionRepository>();
            _actionBinding = actionBinding;
            AvailableDataSources = _bindingActionRepository.GetAll<ISimBoolSourceAction>().Select(a => new SimBoolSourceActionViewModel((ISimBoolSourceAction)a.CreateNew())).ToList();
            CommandClearDataSource = new NotifiedRelayCommand(o => SelectedDataSource = null, o => DataSource != null, this, nameof(DataSource));

            LoadFromModel();
            EnableTouchedTracking();
        }

        public override IEnumerable<ISimBoolSourceActionViewModel> AvailableDataSources { get; }

        public override ICommand CommandClearDataSource { get; }
        public override string ConfigurationSummary { get => DataSource?.ConfigurationSummary ?? ""; protected set => throw new NotSupportedException(); }
        public override ISimBoolSourceActionViewModel? DataSource => _dataSource;

        public override ISimBoolSourceActionViewModel? SelectedDataSource
        {
            get => AvailableDataSources.FirstOrDefault(a => a.UniqueIdentifier == _dataSource?.UniqueIdentifier);
            set
            {
                if (value?.UniqueIdentifier != _dataSource?.UniqueIdentifier)
                {
                    if (_dataSource != null)
                    {
                        RemoveChildren(_dataSource);
                        _dataSource.PropertyChanged -= DataSource_PropertyChanged;
                    }
                    _dataSource = value?.CreateNew();
                    if (_dataSource != null)
                    {
                        _dataSource.PropertyChanged += DataSource_PropertyChanged;
                        AddChildren(_dataSource);
                    }
                    OnPropertyChanged(true);
                    OnPropertyChanged(true, nameof(ConfigurationSummary));
                    OnPropertyChanged(nameof(DataSource));
                }
            }
        }

        protected override void OnApplyChanges()
        {
            _actionBinding.DataSource = DataSource?.Model;
        }

        protected override void OnRevertChanges()
        {
            LoadFromModel();
        }

        private void DataSource_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BindableActionViewModel.ConfigurationSummary))
            {
                OnPropertyChanged(true, nameof(ConfigurationSummary));
            }
        }

        private void LoadFromModel()
        {
            if (_actionBinding.DataSource != null)
            {
                SelectedDataSource = new SimBoolSourceActionViewModel(_actionBinding.DataSource);
            }
            else
            {
                SelectedDataSource = null;
            }
        }
    }
}