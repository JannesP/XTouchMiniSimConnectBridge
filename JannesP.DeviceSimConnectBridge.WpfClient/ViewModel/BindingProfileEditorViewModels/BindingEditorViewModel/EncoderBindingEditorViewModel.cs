using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingActionViewModels;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public class DesignTimeEncoderBindingEditorViewModel : IEncoderBindingEditorViewModel
    {
        private static int _instanceCount = 0;

        public DesignTimeEncoderBindingEditorViewModel() : base(null!, null)
        {
            EncoderClockwiseAction = AvailableActions.Skip(1).First();
            EncoderAntiClockwiseAction = AvailableActions.Skip(2).First();
            ConfigurationSummary = $"{EncoderClockwiseAction.ConfigurationSummary}, {EncoderAntiClockwiseAction.ConfigurationSummary}";
        }

        public override IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; } = new List<ISimpleBindableActionEditorViewModel>
        {
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
        };

        public override ICommand CommandClearAntiClockwiseAction { get; protected set; } = new DesignTimeCommand();
        public override ICommand CommandClearClockwiseAction { get; protected set; } = new DesignTimeCommand();
        public override string ConfigurationSummary { get; protected set; }
        public override ISimpleBindableActionEditorViewModel? EncoderAntiClockwiseAction { get; }
        public override ISimpleBindableActionEditorViewModel? EncoderClockwiseAction { get; }
        public override bool IgnoreSpeed { get; set; } = false;
        public override string Name { get; } = $"Design Time Encoder {++_instanceCount}";
        public override ISimpleBindableActionEditorViewModel? SelectedAntiClockwiseAction { get => EncoderAntiClockwiseAction; set => throw new NotSupportedException(); }
        public override ISimpleBindableActionEditorViewModel? SelectedClockwiseAction { get => EncoderClockwiseAction; set => throw new NotSupportedException(); }

        protected override void OnApplyChanges() => throw new NotSupportedException();

        protected override void OnRevertChanges() => throw new NotSupportedException();
    }

    public class EncoderBindingEditorViewModel : IEncoderBindingEditorViewModel
    {
        private readonly BindingActionRepository _bindingActionRepository;
        private readonly EncoderActionBinding _encoderBinding;

        private bool _ignoreSpeed;
        private ISimpleBindableActionEditorViewModel? _selectedAntiClockwiseAction;
        private ISimpleBindableActionEditorViewModel? _selectedClockwiseAction;

        public EncoderBindingEditorViewModel(IServiceProvider serviceProvider, EncoderActionBinding encoderBinding, IDeviceEncoder? deviceEncoder) : base(encoderBinding, deviceEncoder)
        {
            _bindingActionRepository = serviceProvider.GetRequiredService<BindingActionRepository>();
            _encoderBinding = encoderBinding;
            AvailableActions = _bindingActionRepository.GetAll<ISimpleBindableAction>().Select(b => new SimpleBindableActionEditorViewModel(b)).ToList();
            CommandClearClockwiseAction = new NotifiedRelayCommand(o => SelectedClockwiseAction = null, o => EncoderClockwiseAction != null, this, nameof(EncoderClockwiseAction));
            CommandClearAntiClockwiseAction = new NotifiedRelayCommand(o => SelectedAntiClockwiseAction = null, o => EncoderAntiClockwiseAction != null, this, nameof(EncoderAntiClockwiseAction));

            LoadFromModel();
            EnableTouchedTracking();
        }

        public override IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; }

        public override ICommand CommandClearAntiClockwiseAction { get; protected set; }
        public override ICommand CommandClearClockwiseAction { get; protected set; }

        public override string ConfigurationSummary
        {
            get
            {
                string? configSummaryClockwise = EncoderClockwiseAction?.ConfigurationSummary;
                string? configSummaryAntiClockwise = EncoderAntiClockwiseAction?.ConfigurationSummary;
                if (string.IsNullOrWhiteSpace(configSummaryClockwise) && string.IsNullOrWhiteSpace(configSummaryAntiClockwise))
                {
                    return "";
                }
                else
                {
                    var sb = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(configSummaryClockwise)) sb.Append(configSummaryClockwise);
                    if (!string.IsNullOrWhiteSpace(configSummaryAntiClockwise))
                    {
                        if (sb.Length > 0) sb.Append(", ");
                        sb.Append(configSummaryAntiClockwise);
                    }
                    return sb.ToString();
                }
            }
            protected set => throw new NotSupportedException();
        }

        public override ISimpleBindableActionEditorViewModel? EncoderAntiClockwiseAction => _selectedAntiClockwiseAction;

        public override ISimpleBindableActionEditorViewModel? EncoderClockwiseAction => _selectedClockwiseAction;

        public override bool IgnoreSpeed
        {
            get => _ignoreSpeed;
            set
            {
                if (_ignoreSpeed != value)
                {
                    _ignoreSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public override ISimpleBindableActionEditorViewModel? SelectedAntiClockwiseAction
        {
            get => AvailableActions.FirstOrDefault(a => a.UniqueIdentifier == _selectedAntiClockwiseAction?.UniqueIdentifier);
            set
            {
                if (value?.UniqueIdentifier != _selectedAntiClockwiseAction?.UniqueIdentifier)
                {
                    if (_selectedAntiClockwiseAction != null)
                    {
                        RemoveChildren(_selectedAntiClockwiseAction);
                        _selectedAntiClockwiseAction.PropertyChanged -= Action_PropertyChanged;
                    }
                    _selectedAntiClockwiseAction = value?.CreateNew();
                    if (_selectedAntiClockwiseAction != null)
                    {
                        _selectedAntiClockwiseAction.PropertyChanged += Action_PropertyChanged;
                        AddChildren(_selectedAntiClockwiseAction);
                    }
                    OnPropertyChanged(true);
                    OnPropertyChanged(nameof(EncoderClockwiseAction));
                }
            }
        }

        public override ISimpleBindableActionEditorViewModel? SelectedClockwiseAction
        {
            get => AvailableActions.FirstOrDefault(a => a.UniqueIdentifier == _selectedClockwiseAction?.UniqueIdentifier);
            set
            {
                if (value?.UniqueIdentifier != _selectedClockwiseAction?.UniqueIdentifier)
                {
                    if (_selectedClockwiseAction != null)
                    {
                        RemoveChildren(_selectedClockwiseAction);
                        _selectedClockwiseAction.PropertyChanged -= Action_PropertyChanged;
                    }
                    _selectedClockwiseAction = value?.CreateNew();
                    if (_selectedClockwiseAction != null)
                    {
                        _selectedClockwiseAction.PropertyChanged += Action_PropertyChanged;
                        AddChildren(_selectedClockwiseAction);
                    }
                    OnPropertyChanged(true);
                    OnPropertyChanged(nameof(EncoderClockwiseAction));
                }
            }
        }

        protected override void OnApplyChanges()
        {
            _encoderBinding.IgnoreSpeed = IgnoreSpeed;
            _encoderBinding.TurnClockwise = EncoderClockwiseAction?.Model;
            _encoderBinding.TurnAntiClockwise = EncoderAntiClockwiseAction?.Model;
        }

        protected override void OnRevertChanges()
        {
            LoadFromModel();
        }

        private void Action_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BindableActionViewModel.ConfigurationSummary))
            {
                OnPropertyChanged(true, nameof(ConfigurationSummary));
            }
        }

        private void LoadFromModel()
        {
            IgnoreSpeed = _encoderBinding.IgnoreSpeed;
            if (_encoderBinding.TurnClockwise != null)
            {
                SelectedClockwiseAction = new SimpleBindableActionEditorViewModel(_encoderBinding.TurnClockwise);
            }
            else
            {
                SelectedClockwiseAction = null;
            }
            if (_encoderBinding.TurnAntiClockwise != null)
            {
                SelectedAntiClockwiseAction = new SimpleBindableActionEditorViewModel(_encoderBinding.TurnAntiClockwise);
            }
            else
            {
                SelectedAntiClockwiseAction = null;
            }
        }
    }

    public abstract class IEncoderBindingEditorViewModel : IBindingEditorViewModel
    {
        protected IEncoderBindingEditorViewModel(EncoderActionBinding actionBinding, IDeviceEncoder? deviceButton) : base(actionBinding, deviceButton)
        { }

        public abstract IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; }
        public abstract ICommand CommandClearAntiClockwiseAction { get; protected set; }
        public abstract ICommand CommandClearClockwiseAction { get; protected set; }
        public abstract ISimpleBindableActionEditorViewModel? EncoderAntiClockwiseAction { get; }
        public abstract ISimpleBindableActionEditorViewModel? EncoderClockwiseAction { get; }
        public abstract bool IgnoreSpeed { get; set; }
        public abstract ISimpleBindableActionEditorViewModel? SelectedAntiClockwiseAction { get; set; }
        public abstract ISimpleBindableActionEditorViewModel? SelectedClockwiseAction { get; set; }
    }
}