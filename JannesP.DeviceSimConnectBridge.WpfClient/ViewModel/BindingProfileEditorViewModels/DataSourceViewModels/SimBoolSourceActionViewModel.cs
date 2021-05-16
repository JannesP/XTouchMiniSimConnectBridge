using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.DataSourceViewModels
{
    public class DesignTimeSimBoolSourceActionViewModel : ISimBoolSourceActionViewModel
    {
        private static int _instanceCount = 0;

        public DesignTimeSimBoolSourceActionViewModel() : base(new DesignTimeBindableAction())
        {
        }

        public override string ConfigurationSummary => "SimVar: 'AUTOPILOT MASTER'";
        public override string Description => "Super cool description!";
        public override string Name { get; } = $"Design Time SimBoolSource {++_instanceCount}";
        public override string UniqueIdentifier => nameof(DesignTimeSimBoolSourceActionViewModel);

        public override ISimBoolSourceActionViewModel CreateNew() => throw new NotSupportedException();
    }

    public abstract class ISimBoolSourceActionViewModel : BindableActionViewModel
    {
        public ISimBoolSourceActionViewModel(IBindableAction action) : base(action)
        {
        }

        public new ISimBoolSourceAction Model => (ISimBoolSourceAction)base.Model;

        public abstract ISimBoolSourceActionViewModel CreateNew();
    }

    public class SimBoolSourceActionViewModel : ISimBoolSourceActionViewModel
    {
        private readonly ISimBoolSourceAction _simBoolSourceAction;

        public SimBoolSourceActionViewModel(ISimBoolSourceAction simBoolSourceAction) : base(simBoolSourceAction)
        {
            _simBoolSourceAction = simBoolSourceAction;
        }

        public override string UniqueIdentifier => _simBoolSourceAction.UniqueIdentifier;

        public override ISimBoolSourceActionViewModel CreateNew() => new SimBoolSourceActionViewModel(_simBoolSourceAction);
    }
}