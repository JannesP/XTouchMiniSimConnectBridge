using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions
{
    public interface IBindableAction
    {
        string Name { get; }
        string Description { get; }
        bool IsInitialized { get; }
        void Initialize(IServiceProvider serviceProvider);
    }

    public interface ISimpleBindableAction : IBindableAction
    {
        Task ExecuteAsync();
    }

    internal static class IBindableActionExtensions
    {
        public static IEnumerable<PropertyInfo> GetSettings(this IBindableAction action)
        {
            Type t = action.GetType();
            PropertyInfo[]? props = t.GetProperties(BindingFlags.Public);
            foreach (PropertyInfo? prop in props)
            {
                var customAttr = prop.GetCustomAttribute<ActionSettingAttribute>();
                if (customAttr != null)
                {
                    yield return prop;
                }
            }
        }
    }
}
