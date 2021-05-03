using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels
{
    public abstract class IBindingProfileDeviceEditorViewModel : ViewModelBase
    {
        public abstract string Name { get; }

        public abstract IEnumerable<IBindingListViewModel> BindingTypes { get; }
    }

    public class DesignTimeBindingProfileDeviceEditorViewModel : IBindingProfileDeviceEditorViewModel
    {
        public override string Name { get; } = Guid.NewGuid().ToString();
        public override IEnumerable<IBindingListViewModel> BindingTypes { get; } = new List<IBindingListViewModel>() 
        {
            new DesignTimeBindingListViewModel("Buttons")
            {
                Editors = new List<DesignTimeButtonBindingEditorViewModel>()
                {
                    new DesignTimeButtonBindingEditorViewModel(),
                    new DesignTimeButtonBindingEditorViewModel(),
                }
            },
            new DesignTimeBindingListViewModel("Encoders")
            {
                Editors = new List<DesignTimeEncoderBindingEditorViewModel>()
                {
                    new DesignTimeEncoderBindingEditorViewModel(),
                    new DesignTimeEncoderBindingEditorViewModel(),
                }
            },
            new DesignTimeBindingListViewModel("Faders")
            {
                Editors = new List<DesignTimeFaderBindingEditorViewModel>()
                {
                    new DesignTimeFaderBindingEditorViewModel(),
                    new DesignTimeFaderBindingEditorViewModel(),
                }
            },
            new DesignTimeBindingListViewModel("Leds")
            {
                Editors = new List<DesignTimeLedBindingEditorViewModel>()
                {
                    new DesignTimeLedBindingEditorViewModel(),
                    new DesignTimeLedBindingEditorViewModel(),
                }
            }
        };
    }

    public abstract class IBindingListViewModel : ViewModelBase
    {
        public abstract string CategoryName { get; }
        public abstract IEnumerable<IBindingEditorViewModel> Editors { get; set; }
    }

    public class DesignTimeBindingListViewModel : IBindingListViewModel
    {
        public override string CategoryName { get; }

        public DesignTimeBindingListViewModel(string categoryName)
        {
            CategoryName = categoryName;
            Editors = new List<IBindingEditorViewModel>();
        }

        public override IEnumerable<IBindingEditorViewModel> Editors { get; set; }
    }
}
