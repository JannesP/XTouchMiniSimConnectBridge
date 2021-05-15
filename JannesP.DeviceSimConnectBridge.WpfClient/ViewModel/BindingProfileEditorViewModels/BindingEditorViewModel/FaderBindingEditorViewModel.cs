using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public abstract class IFaderBindingEditorViewModel : IBindingEditorViewModel
    {
        protected IFaderBindingEditorViewModel(IDeviceFader? deviceControl) : base(null!, deviceControl)
        { }
    }

    public class DesignTimeFaderBindingEditorViewModel : IFaderBindingEditorViewModel
    {
        public DesignTimeFaderBindingEditorViewModel() : base(null)
        { }

        private static int _instanceCount = 0;
        public override string Name { get; } = $"Design Time Fader {++_instanceCount}";

        protected override void OnApplyChanges() => throw new NotImplementedException();
        protected override void OnRevertChanges() => throw new NotImplementedException();
    }
}
