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
        string UniqueIdentifier { get; }
        bool IsInitialized { get; }
        void Initialize(IServiceProvider serviceProvider);
        void Deactivate();
    }

    public interface ISimpleBindableAction : IInputAction
    {
        Task ExecuteAsync();
    }

    public record BindableActionSetting(object Target, PropertyInfo Property, ActionSettingAttribute Attribute)
    {
        public object? Value { get => Property.GetValue(Target); set => Property.SetValue(Target, value); }
    }

    public static class IBindableActionExtensions
    {
        public static IEnumerable<BindableActionSetting> GetSettings(this IBindableAction action)
        {
            Type t = action.GetType();
            PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                ActionSettingAttribute? attribute = prop.GetCustomAttribute<ActionSettingAttribute>();
                if (attribute != null)
                {
                    yield return new BindableActionSetting(action, prop, attribute);
                }
            }
        }

        public static IBindableAction CreateNew(this IBindableAction action)
        {
            Type t = action.GetType();
            return Activator.CreateInstance(t) as IBindableAction ?? throw new Exception($"Activator.CreateInstance returned null for {t.FullName}");
        }
    }
}
