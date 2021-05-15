﻿using System;
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
    public abstract class IButtonBindingEditorViewModel : IBindingEditorViewModel
    {
        protected IButtonBindingEditorViewModel(ActionBinding model, IDeviceButton? deviceButton) : base(model, deviceButton)
        { }

        public abstract IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; }
        public abstract ISimpleBindableActionEditorViewModel? ButtonPressAction { get; }
        public abstract ISimpleBindableActionEditorViewModel? SelectedAction { get; set; }
        public abstract bool TriggerOnRelease { get; set; }
    }

    public class DesignTimeButtonBindingEditorViewModel : IButtonBindingEditorViewModel
    {
        private static int _instanceCount = 0;
        public override string Name { get; } = $"Design Time Button {++_instanceCount}";

        public DesignTimeButtonBindingEditorViewModel() : base(null!, null)
        {
            ButtonPressAction = AvailableActions.Skip(1).First();
        }

        public override IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; } = new List<ISimpleBindableActionEditorViewModel>
        {
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
        };

        public override bool TriggerOnRelease { get; set; } = false;
        public override ISimpleBindableActionEditorViewModel? ButtonPressAction { get; }
        public override ISimpleBindableActionEditorViewModel? SelectedAction { get => ButtonPressAction; set => throw new NotSupportedException(); }

        protected override void OnApplyChanges() => throw new NotSupportedException();
        protected override void OnRevertChanges() => throw new NotSupportedException();
    }

    public class ButtonBindingEditorViewModel : IButtonBindingEditorViewModel
    {
        private readonly BindingActionRepository _bindingActionRepository;
        private readonly ButtonActionBinding _buttonBinding;

        private ISimpleBindableActionEditorViewModel? _buttonPressAction;
        private bool _triggerOnRelease = false;

        public ButtonBindingEditorViewModel(IServiceProvider serviceProvider, ButtonActionBinding buttonBinding, IDeviceButton? deviceButton) : base(buttonBinding, deviceButton)
        {
            _bindingActionRepository = serviceProvider.GetRequiredService<BindingActionRepository>();
            _buttonBinding = buttonBinding;
            AvailableActions = _bindingActionRepository.GetAll<ISimpleBindableAction>().Select(b => new SimpleBindableActionEditorViewModel(b)).ToList();

            LoadFromModel();
        }

        private void LoadFromModel()
        {
            _triggerOnRelease = _buttonBinding.TriggerOnRelease;
            if (_buttonBinding.ButtonPressed != null)
            {
                SelectedAction = new SimpleBindableActionEditorViewModel(_buttonBinding.ButtonPressed);
            }
        }

        public override IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; }
        
        public override ISimpleBindableActionEditorViewModel? SelectedAction
        {
            get => AvailableActions.FirstOrDefault(a => a.UniqueIdentifier == _buttonPressAction?.UniqueIdentifier);
            set
            {
                if (value?.GetType() != _buttonPressAction?.GetType())
                {
                    if (_buttonPressAction != null) RemoveChildren(_buttonPressAction);
                    _buttonPressAction = value?.CreateNew();
                    if (_buttonPressAction != null) AddChildren(_buttonPressAction);
                    OnPropertyChanged(true);
                    OnPropertyChanged(nameof(ButtonPressAction));
                }
            }
        }

        public override ISimpleBindableActionEditorViewModel? ButtonPressAction => _buttonPressAction;

        public override bool TriggerOnRelease
        {
            get => _triggerOnRelease;
            set
            {
                if (_triggerOnRelease != value)
                {
                    _triggerOnRelease = value;
                    OnPropertyChanged();
                }
            }
        }

        protected override void OnApplyChanges()
        {
            _buttonBinding.TriggerOnRelease = TriggerOnRelease;
            _buttonBinding.ButtonPressed = ButtonPressAction?.Model;
        }

        protected override void OnRevertChanges()
        {
            LoadFromModel();
        }
    }
}
