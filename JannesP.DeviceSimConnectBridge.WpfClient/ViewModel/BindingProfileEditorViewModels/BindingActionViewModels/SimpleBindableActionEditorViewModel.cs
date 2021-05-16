using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingActionViewModels
{
    public class DesignTimeSimpleBindableActionEditorViewModel : ISimpleBindableActionEditorViewModel
    {
        private static int _instanceCount = 0;

        public DesignTimeSimpleBindableActionEditorViewModel() : base(new DesignTimeBindableAction())
        {
        }

        public override string ConfigurationSummary => "Command: AP_HEADING_INC";
        public override string Description => "Super cool description!";
        public override string Name { get; } = $"Design Time SimpleBindableAction {++_instanceCount}";
        public override string UniqueIdentifier => nameof(DesignTimeSimpleBindableActionEditorViewModel);

        public override ISimpleBindableActionEditorViewModel CreateNew() => throw new NotSupportedException();
    }

    public abstract class ISimpleBindableActionEditorViewModel : BindableActionViewModel
    {
        public ISimpleBindableActionEditorViewModel(ISimpleBindableAction action) : base(action)
        {
        }

        public new ISimpleBindableAction Model => (ISimpleBindableAction)base.Model;

        public abstract ISimpleBindableActionEditorViewModel CreateNew();
    }

    public class SimpleBindableActionEditorViewModel : ISimpleBindableActionEditorViewModel
    {
        private readonly ISimpleBindableAction _action;

        public SimpleBindableActionEditorViewModel(ISimpleBindableAction action) : base(action)
        {
            _action = action;
        }

        public override string UniqueIdentifier => _action.UniqueIdentifier;

        public override ISimpleBindableActionEditorViewModel CreateNew() => new SimpleBindableActionEditorViewModel(_action);
    }
}