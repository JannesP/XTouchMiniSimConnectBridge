using System;
using System.Reflection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Extensions
{
    public static class ObjectReflectionExtensions
    {
        public static TRes? GetPropertyValueByName<TRes>(this object obj, string propertyName)
        {
            PropertyInfo? propInfo = obj.GetType().GetProperty(propertyName);
            if (propInfo == null) throw new MissingMemberException(obj.GetType().FullName, propertyName);
            object? value = propInfo.GetValue(obj);
            return value == null ? default : (TRes)value;
        }

        public static void SetPropertyValueByName(this object obj, string propertyName, object? value)
        {
            PropertyInfo? propInfo = obj.GetType().GetProperty(propertyName);
            if (propInfo == null) throw new MissingMemberException(obj.GetType().FullName, propertyName);
            propInfo.SetValue(obj, value);
        }
    }
}