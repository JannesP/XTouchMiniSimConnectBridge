using System.Collections.Generic;
using System.Linq;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.DataSources;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.SimConnectActions;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Repositories
{
    public class BindingActionRepository
    {
        private readonly List<IBindableAction> _actionList;

        public BindingActionRepository()
        {
            _actionList = new List<IBindableAction>
            {
                //simple actions
                new SimConnectActionSimEvent(),
                new MsfsFireHEvent(),

                //data sources
                new SimVarBoolDataSource(),
            };
        }

        public IEnumerable<TRes> GetAll<TRes>() where TRes : IBindableAction
            => _actionList.Where(a => a is TRes).Cast<TRes>();
    }
}