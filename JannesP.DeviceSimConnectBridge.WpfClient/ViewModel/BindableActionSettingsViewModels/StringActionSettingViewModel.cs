using System;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels
{
    public class StringActionSettingViewModel : ActionSettingViewModel
    {
        internal StringActionSettingViewModel(BindableActionSetting setting) : base(setting)
        { if (setting.Attribute.GetType() != typeof(StringActionSettingAttribute)) throw new ArgumentException("Only StringActionSettingAttribute settings are allowed.", nameof(setting)); }
    }
}