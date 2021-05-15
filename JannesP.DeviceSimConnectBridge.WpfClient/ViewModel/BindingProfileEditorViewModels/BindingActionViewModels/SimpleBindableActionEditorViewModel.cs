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

        public abstract string ConfigurationSummary { get; }
        public new ISimpleBindableAction Model => (ISimpleBindableAction)base.Model;
        public abstract string UniqueIdentifier { get; }

        public abstract ISimpleBindableActionEditorViewModel CreateNew();

        protected void OnConfigurationPropertyChanged([CallerMemberName] string? configPropertyName = null)
        {
            OnPropertyChanged(true, configPropertyName);
            OnPropertyChanged(nameof(ConfigurationSummary));
        }
    }

    public class SimpleBindableActionEditorViewModel : ISimpleBindableActionEditorViewModel
    {
        private readonly ISimpleBindableAction _action;

        public SimpleBindableActionEditorViewModel(ISimpleBindableAction action) : base(action)
        {
            _action = action;
        }

        public override string ConfigurationSummary
        {
            get
            {
                StringBuilder sb = new();
                IEnumerable<BindableActionSetting> settings = _action.GetSettings();
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

        public override string UniqueIdentifier => _action.UniqueIdentifier;

        public override ISimpleBindableActionEditorViewModel CreateNew() => new SimpleBindableActionEditorViewModel(_action);
    }
}