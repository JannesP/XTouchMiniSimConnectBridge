using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels
{
    public abstract class ActionSettingViewModel : RevertibleViewModelBase
    {
        private object? _value;
        protected ActionSettingViewModel(BindableActionSetting setting)
        {
            Setting = setting;
            LoadFromModel();
        }

        public string Description => Setting.Attribute.Description;
        public string Name => Setting.Attribute.Name;
        [CustomValidation(typeof(ViewModelBase), nameof(ViewModelBase.ValidateProperty))]
        public object? Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        protected BindableActionSetting Setting { get; }
        public static ActionSettingViewModel Create(BindableActionSetting setting)
        {
            return setting.Attribute switch
            {
                StringActionSettingAttribute => new StringActionSettingViewModel(setting),
                IntActionSettingAttribute => new IntActionSettingViewModel(setting),
                _ => throw new NotImplementedException()
            };
        }

        public override string? OnValidateProperty(string propertyName, object? value)
        {
            string? result = null;
            switch (propertyName)
            {
                case nameof(Value):
                    result = Setting.Attribute.ValidateValue(value);
                    break;
            }
            return result;
        }

        protected override void OnApplyChanges() => Setting.Value = _value;

        protected override void OnRevertChanges() => LoadFromModel();

        private void LoadFromModel()
        {
            Value = Setting.Value;
        }
    }

    public abstract class BindableActionViewModel : RevertibleViewModelBase
    {
        protected BindableActionViewModel(IBindableAction bindableAction)
        {
            Model = bindableAction;
            Settings = Model.GetSettings().Select(s => ActionSettingViewModel.Create(s)).ToArray();
            AddChildren(Settings);
        }

        public virtual string Description => Model.Description;
        public virtual string Name => Model.Name;
        public IBindableAction Model { get; }
        public IEnumerable<ActionSettingViewModel> Settings { get; }

        protected override void OnApplyChanges() { /* nothing to do here */ }
        protected override void OnRevertChanges() { /* nothing to do here */ }
    }

    public class DesignTimeBindableAction : IBindableAction, ISimpleBindableAction
    {
        public string Description => "Description for the DesignTime BindableAction";

        public bool IsInitialized => false;

        public string Name => "DesignTime BindableAction";

        [IntActionSetting("Int Setting", "This is a description for a DesignTime int setting.")]
        public int TestInt { get; set; } = 259;

        [StringActionSetting("String Setting", "This is a description for a DesignTime string setting.")]
        public string TestString { get; set; } = "SUPER EVENT";
        public string UniqueIdentifier => nameof(DesignTimeBindableAction);
        public Task DeactivateAsync() => throw new NotSupportedException();
        public Task ExecuteAsync() => throw new NotImplementedException();
        public Task InitializeAsync(IServiceProvider serviceProvider) => throw new NotSupportedException();
    }

    public class DesignTimeBindableActionViewModel : BindableActionViewModel
    {
        public DesignTimeBindableActionViewModel() : base(new DesignTimeBindableAction()) { }

        protected override void OnApplyChanges() => throw new NotSupportedException();
        protected override void OnRevertChanges() => throw new NotSupportedException();
    }
}
