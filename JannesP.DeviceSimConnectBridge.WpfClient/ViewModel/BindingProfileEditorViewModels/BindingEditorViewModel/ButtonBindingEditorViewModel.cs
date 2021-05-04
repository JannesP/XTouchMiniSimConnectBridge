using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public abstract class ISimpleBindableActionEditorViewModel
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string ConfigurationSummary { get; }
    }

    public class DesignTimeSimpleBindableActionEditorViewModel : ISimpleBindableActionEditorViewModel
    {
        private static int _instanceCount = 0;
        public override string Name { get; } = $"Design Time SimpleBindableAction {++_instanceCount}";
        public override string Description => "Super cool description!";
        public override string ConfigurationSummary => "Command: AP_HEADING_INC";
    }


    public abstract class IButtonBindingEditorViewModel : IBindingEditorViewModel
    {
        public abstract IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; }
        public abstract ISimpleBindableActionEditorViewModel ButtonPressAction { get; set; }
    }

    public class DesignTimeButtonBindingEditorViewModel : IButtonBindingEditorViewModel
    {
        private static int _instanceCount = 0;
        public override string Name { get; } = $"Design Time Button {++_instanceCount}";

        public DesignTimeButtonBindingEditorViewModel()
        {
            ButtonPressAction = AvailableActions.Skip(1).First();
        }

        public override IEnumerable<ISimpleBindableActionEditorViewModel> AvailableActions { get; } = new List<ISimpleBindableActionEditorViewModel> 
        {
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
            new DesignTimeSimpleBindableActionEditorViewModel(),
        };

        public override ISimpleBindableActionEditorViewModel ButtonPressAction { get; set; }
    }
}
