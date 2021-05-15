using System;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public class DesignTimeFaderBindingEditorViewModel : IFaderBindingEditorViewModel
    {
        private static int _instanceCount = 0;

        public DesignTimeFaderBindingEditorViewModel() : base(null)
        { }

        public override string Name { get; } = $"Design Time Fader {++_instanceCount}";

        protected override void OnApplyChanges() => throw new NotImplementedException();

        protected override void OnRevertChanges() => throw new NotImplementedException();
    }

    public abstract class IFaderBindingEditorViewModel : IBindingEditorViewModel
    {
        protected IFaderBindingEditorViewModel(IDeviceFader? deviceControl) : base(null!, deviceControl)
        { }
    }
}