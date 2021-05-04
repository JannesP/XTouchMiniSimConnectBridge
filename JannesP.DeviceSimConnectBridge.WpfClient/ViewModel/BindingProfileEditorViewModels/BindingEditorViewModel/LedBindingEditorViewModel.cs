using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public abstract class ILedBindingEditorViewModel : IBindingEditorViewModel
    {

    }

    public class DesignTimeLedBindingEditorViewModel : ILedBindingEditorViewModel
    {
        private static int _instanceCount = 0;
        public override string Name { get; } = $"Design Time Led {++_instanceCount}";
    }
}
