using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels
{
    public interface IBindingProfileEditorViewModel
    { 
        ObservableCollection<IBindingProfileDeviceEditorViewModel> Devices { get; }
    }

    public class DesignTimeBindingProfileEditorViewModel : IBindingProfileEditorViewModel
    {
        public ObservableCollection<IBindingProfileDeviceEditorViewModel> Devices { get; } = new ObservableCollection<IBindingProfileDeviceEditorViewModel>()
        {
            new DesignTimeBindingProfileDeviceEditorViewModel(),
            new DesignTimeBindingProfileDeviceEditorViewModel(),
            new DesignTimeBindingProfileDeviceEditorViewModel(),
        };
    }

}
