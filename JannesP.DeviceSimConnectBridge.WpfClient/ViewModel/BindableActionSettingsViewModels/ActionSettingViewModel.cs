using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels
{
    public abstract class ActionSettingViewModel : ViewModelBase
    {
        protected BindableActionSetting Setting { get; }

        protected ActionSettingViewModel(BindableActionSetting setting)
        {
            Setting = setting;
        }

        public string Name => Setting.Attribute.Name;
        public string Description => Setting.Attribute.Description;
        public object? Value { get => Setting.Value; set => Setting.Value = value; }

        public static ActionSettingViewModel Create(BindableActionSetting setting)
        {
            return setting.Attribute switch
            {
                StringActionSettingAttribute => new StringActionSettingViewModel(setting),
                IntActionSettingAttribute => new IntActionSettingViewModel(setting),
                _ => throw new NotImplementedException()
            };
        }
    }

    public abstract class BindableActionViewModel : ViewModelBase
    {
        private readonly IBindableAction _bindableAction;
        public IEnumerable<ActionSettingViewModel> Settings { get; }
        public virtual string Name => _bindableAction.Name;
        public virtual string Description => _bindableAction.Description;

        protected BindableActionViewModel(IBindableAction bindableAction)
        {
            _bindableAction = bindableAction;
            Settings = _bindableAction.GetSettings().Select(s => ActionSettingViewModel.Create(s)).ToArray();
        }
    }

    public class DesignTimeBindableActionViewModel : BindableActionViewModel
    {
        public DesignTimeBindableActionViewModel() : base(new DesignTimeBindableAction()) { }
    }

    public class DesignTimeBindableAction : IBindableAction
    {
        [StringActionSetting("String Setting", "This is a description for a DesignTime string setting.")]
        public string TestString { get; set; } = "SUPER EVENT";

        [IntActionSetting("Int Setting", "This is a description for a DesignTime int setting.")]
        public int TestInt { get; set; } = 259;

        public string Name => "DesignTime BindableAction";
        public string Description => "Description for the DesignTime BindableAction";
        public string UniqueIdentifier => nameof(DesignTimeBindableAction);
        public bool IsInitialized => false;
        public void Deactivate() => throw new NotSupportedException();
        public void Initialize(IServiceProvider serviceProvider) => throw new NotSupportedException();
    }
}
