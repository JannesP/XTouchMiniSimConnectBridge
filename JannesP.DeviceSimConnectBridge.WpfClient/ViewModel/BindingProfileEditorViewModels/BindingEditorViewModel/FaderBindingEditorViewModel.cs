﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels.BindingEditorViewModel
{
    public abstract class IFaderBindingEditorViewModel : IBindingEditorViewModel
    {

    }

    public class DesignTimeFaderBindingEditorViewModel : IFaderBindingEditorViewModel
    {
        private static int _instanceCount = 0;
        public override string Name { get; } = $"Design Time Fader {++_instanceCount}";
    }
}
