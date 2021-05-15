using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.DataSourceViewModels
{
    public abstract class ISimBoolSourceActionViewModel : BindableActionViewModel
    {
        public ISimBoolSourceActionViewModel(IBindableAction action) : base(action) { }

        public abstract string UniqueIdentifier { get; }
        public abstract ISimBoolSourceActionViewModel CreateNew();
        public abstract string ConfigurationSummary { get; }
        protected void OnConfigurationPropertyChanged([CallerMemberName] string? configPropertyName = null)
        {
            OnPropertyChanged(configPropertyName);
            OnPropertyChanged(nameof(ConfigurationSummary));
        }
        public new ISimBoolSourceAction Model => (ISimBoolSourceAction)base.Model;
    }

    public class DesignTimeSimBoolSourceActionViewModel : ISimBoolSourceActionViewModel
    {
        public DesignTimeSimBoolSourceActionViewModel() : base(new DesignTimeBindableAction()) { }

        private static int _instanceCount = 0;
        public override string Name { get; } = $"Design Time SimBoolSource {++_instanceCount}";
        public override string Description => "Super cool description!";
        public override string ConfigurationSummary => "SimVar: 'AUTOPILOT MASTER'";
        public override string UniqueIdentifier => nameof(DesignTimeSimBoolSourceActionViewModel);

        public override ISimBoolSourceActionViewModel CreateNew() => throw new NotSupportedException();
    }

    public class SimBoolSourceActionViewModel : ISimBoolSourceActionViewModel
    {
        private readonly ISimBoolSourceAction _simBoolSourceAction;

        public SimBoolSourceActionViewModel(ISimBoolSourceAction simBoolSourceAction) : base(simBoolSourceAction)
        {
            _simBoolSourceAction = simBoolSourceAction;
        }

        public override string Name => _simBoolSourceAction.Name;

        public override string Description => _simBoolSourceAction.Description;

        public override string UniqueIdentifier => _simBoolSourceAction.UniqueIdentifier;

        public override string ConfigurationSummary 
        {
            get
            {
                StringBuilder sb = new();
                IEnumerable<BindableActionSetting> settings = _simBoolSourceAction.GetSettings();
                foreach (BindableActionSetting setting in settings)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(setting.Attribute.Name)
                        .Append(": '")
                        .Append(setting.Value?.ToString() ?? "")
                        .Append('\'');
                }
                return sb.ToString();
            }
        }

        public override ISimBoolSourceActionViewModel CreateNew() => throw new NotImplementedException();
    }
}
