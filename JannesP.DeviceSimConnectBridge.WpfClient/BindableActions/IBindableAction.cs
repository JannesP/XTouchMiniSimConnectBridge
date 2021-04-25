using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions
{
    interface IBindableAction
    {
        string Name { get; }
        string Description { get; }
        Task ExecuteAsync();
    }
}
