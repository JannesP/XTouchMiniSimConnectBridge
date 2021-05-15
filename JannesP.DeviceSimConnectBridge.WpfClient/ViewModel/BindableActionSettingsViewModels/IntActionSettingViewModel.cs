using System;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels
{
    public class IntActionSettingViewModel : ActionSettingViewModel
    {
        internal IntActionSettingViewModel(BindableActionSetting setting) : base(setting)
        { if (setting.Attribute.GetType() != typeof(IntActionSettingAttribute)) throw new ArgumentException("Only IntActionSettingAttribute settings are allowed.", nameof(setting)); }
    }
}