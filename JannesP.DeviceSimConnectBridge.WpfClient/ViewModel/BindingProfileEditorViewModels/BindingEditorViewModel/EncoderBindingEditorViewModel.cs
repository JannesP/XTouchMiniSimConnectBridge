using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingActionViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public abstract class IEncoderBindingEditorViewModel : IBindingEditorViewModel
    {
        protected IEncoderBindingEditorViewModel(EncoderActionBinding actionBinding, IDeviceEncoder? deviceButton) : base(actionBinding, deviceButton)
        { }

        public abstract bool IgnoreSpeed { get; set; }

        public abstract IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; }

        public abstract ISimpleBindableActionEditorViewModel? EncoderClockwiseAction { get; }
        public abstract ISimpleBindableActionEditorViewModel? SelectedClockwiseAction { get; set; }

        public abstract ISimpleBindableActionEditorViewModel? EncoderAntiClockwiseAction { get; }
        public abstract ISimpleBindableActionEditorViewModel? SelectedAntiClockwiseAction { get; set; }

    }

    public class DesignTimeEncoderBindingEditorViewModel : IEncoderBindingEditorViewModel
    {
        private static int _instanceCount = 0;
        public override string Name { get; } = $"Design Time Encoder {++_instanceCount}";

        public DesignTimeEncoderBindingEditorViewModel() : base(null!, null)
        {
            EncoderClockwiseAction = AvailableActions.Skip(1).First();
            EncoderAntiClockwiseAction = AvailableActions.Skip(2).First();
        }

        public override IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; } = new List<ISimpleBindableActionEditorViewModel> 
        {
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
        };

        public override bool IgnoreSpeed { get; set; } = false;

        public override ISimpleBindableActionEditorViewModel? EncoderClockwiseAction { get; }

        public override ISimpleBindableActionEditorViewModel? SelectedClockwiseAction { get => EncoderClockwiseAction; set => throw new NotSupportedException(); }

        public override ISimpleBindableActionEditorViewModel? EncoderAntiClockwiseAction { get; }

        public override ISimpleBindableActionEditorViewModel? SelectedAntiClockwiseAction { get => EncoderAntiClockwiseAction; set => throw new NotSupportedException(); }

        protected override void OnApplyChanges() => throw new NotSupportedException();
        protected override void OnRevertChanges() => throw new NotSupportedException();
    }

    public class EncoderBindingEditorViewModel : IEncoderBindingEditorViewModel
    {
        private readonly BindingActionRepository _bindingActionRepository;
        private readonly EncoderActionBinding _encoderBinding;

        private bool _ignoreSpeed;
        private ISimpleBindableActionEditorViewModel? _selectedClockwiseAction;
        private ISimpleBindableActionEditorViewModel? _selectedAntiClockwiseAction;

        public EncoderBindingEditorViewModel(IServiceProvider serviceProvider, EncoderActionBinding encoderBinding, IDeviceEncoder? deviceEncoder) : base(encoderBinding, deviceEncoder)
        {
            _bindingActionRepository = serviceProvider.GetRequiredService<BindingActionRepository>();
            _encoderBinding = encoderBinding;
            AvailableActions = _bindingActionRepository.GetAll<ISimpleBindableAction>().Select(b => new SimpleBindableActionEditorViewModel(b)).ToList();

            LoadFromModel();
        }

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

        private void LoadFromModel()
        {
            IgnoreSpeed = _encoderBinding.IgnoreSpeed;
            if (_encoderBinding.TurnClockwise != null)
            {
                SelectedClockwiseAction = new SimpleBindableActionEditorViewModel(_encoderBinding.TurnClockwise);
            }
            if (_encoderBinding.TurnAntiClockwise != null)
            {
                SelectedAntiClockwiseAction = new SimpleBindableActionEditorViewModel(_encoderBinding.TurnAntiClockwise);
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

        public override IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; }

        public override ISimpleBindableActionEditorViewModel? EncoderClockwiseAction => _selectedClockwiseAction;

        public override ISimpleBindableActionEditorViewModel? SelectedClockwiseAction
        {
            get => AvailableActions.FirstOrDefault(a => a.UniqueIdentifier == _selectedClockwiseAction?.UniqueIdentifier);
            set
            {
                if (value?.GetType() != _selectedClockwiseAction?.GetType())
                {
                    if (_selectedClockwiseAction != null) RemoveChildren(_selectedClockwiseAction);
                    _selectedClockwiseAction = value?.CreateNew();
                    if (_selectedClockwiseAction != null) AddChildren(_selectedClockwiseAction);
                    OnPropertyChanged(true);
                    OnPropertyChanged(nameof(EncoderClockwiseAction));
                }
            }
        }

        public override ISimpleBindableActionEditorViewModel? EncoderAntiClockwiseAction => _selectedAntiClockwiseAction;

        public override ISimpleBindableActionEditorViewModel? SelectedAntiClockwiseAction
        {
            get => AvailableActions.FirstOrDefault(a => a.UniqueIdentifier == _selectedAntiClockwiseAction?.UniqueIdentifier);
            set
            {
                if (value?.GetType() != _selectedAntiClockwiseAction?.GetType())
                {
                    if (_selectedAntiClockwiseAction != null) RemoveChildren(_selectedAntiClockwiseAction);
                    _selectedAntiClockwiseAction = value?.CreateNew();
                    if (_selectedAntiClockwiseAction != null) AddChildren(_selectedAntiClockwiseAction);
                    OnPropertyChanged(true);
                    OnPropertyChanged(nameof(EncoderClockwiseAction));
                }
            }
        }
    }
}
