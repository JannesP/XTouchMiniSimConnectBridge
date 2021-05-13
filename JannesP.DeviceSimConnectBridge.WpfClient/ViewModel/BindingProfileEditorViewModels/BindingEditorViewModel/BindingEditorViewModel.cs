using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public abstract class IBindingEditorViewModel : ViewModelBase
    {
        private readonly IDeviceControl? _control;

        protected IBindingEditorViewModel(IDeviceControl? control)
        {
            _control = control;
        }
        public virtual string Name => _control?.Name ?? "<Device not available>";
        public abstract ActionBinding CreateModel();
    }
}
