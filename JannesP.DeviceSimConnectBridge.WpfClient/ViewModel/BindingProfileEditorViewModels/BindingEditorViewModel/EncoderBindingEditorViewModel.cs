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
        protected IEncoderBindingEditorViewModel(IDeviceEncoder? deviceButton) : base(deviceButton)
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

        public DesignTimeEncoderBindingEditorViewModel() : base(null)
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

        public override ActionBinding CreateModel() => throw new NotSupportedException();
    }

    public class EncoderBindingEditorViewModel : IEncoderBindingEditorViewModel
    {
        private readonly BindingActionRepository _bindingActionRepository;
        private readonly IDeviceEncoder? _deviceEncoder;
        private readonly ActionBinding? _actionBinding;

        private bool _ignoreSpeed;
        private ISimpleBindableActionEditorViewModel? _selectedClockwiseAction;
        private ISimpleBindableActionEditorViewModel? _selectedAntiClockwiseAction;

        public EncoderBindingEditorViewModel(IServiceProvider serviceProvider, IDeviceEncoder? deviceEncoder, ActionBinding? actionBinding) : base(deviceEncoder)
        {
            _bindingActionRepository = serviceProvider.GetRequiredService<BindingActionRepository>();
            _deviceEncoder = deviceEncoder;
            _actionBinding = actionBinding;
            AvailableActions = _bindingActionRepository.GetAll<ISimpleBindableAction>().Select(b => new SimpleBindableActionEditorViewModel(b)).ToList();
            if (actionBinding is EncoderActionBinding binding)
            {
                _ignoreSpeed = binding.IgnoreSpeed;
                if (binding.TurnClockwise != null)
                {
                    _selectedClockwiseAction = new SimpleBindableActionEditorViewModel(binding.TurnClockwise);
                }
                if (binding.TurnAntiClockwise != null)
                {
                    _selectedAntiClockwiseAction = new SimpleBindableActionEditorViewModel(binding.TurnAntiClockwise);
                }
            }
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

        public override IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; }

        public override ISimpleBindableActionEditorViewModel? EncoderClockwiseAction => _selectedClockwiseAction;

        public override ISimpleBindableActionEditorViewModel? SelectedClockwiseAction
        {
            get => AvailableActions.FirstOrDefault(a => a.UniqueIdentifier == _selectedClockwiseAction?.UniqueIdentifier);
            set
            {
                if (value?.GetType() != _selectedClockwiseAction?.GetType())
                {
                    _selectedClockwiseAction = value?.CreateNew();
                    OnPropertyChanged();
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
                    _selectedAntiClockwiseAction = value?.CreateNew();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EncoderClockwiseAction));
                }
            }
        }

        public override ActionBinding CreateModel() => throw new NotImplementedException();
    }
}
