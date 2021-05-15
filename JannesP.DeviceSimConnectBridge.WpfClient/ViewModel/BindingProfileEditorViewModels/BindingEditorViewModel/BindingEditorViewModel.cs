using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public abstract class IBindingEditorViewModel : RevertibleViewModelBase
    {
        private readonly IDeviceControl? _control;

        protected IBindingEditorViewModel(ActionBinding model, IDeviceControl? control)
        {
            Model = model;
            _control = control;
        }

        public ActionBinding Model { get; }
        public virtual string Name => _control?.Name ?? "<Device not available>";
    }
}