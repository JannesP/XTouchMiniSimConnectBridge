using System;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    internal abstract class ActionSettingAttribute : Attribute
    {
        public ActionSettingAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    internal class StringActionSettingAttribute : ActionSettingAttribute
    {
        public StringActionSettingAttribute(string name) : base(name) { }

        public virtual bool CanBeEmpty { get; set; } = true;
        public virtual int? MaxLength { get; set; } = null;
        public virtual int? MinLength { get; set; } = null;

        public string? ValidateValue(string value)
        {
            if (!CanBeEmpty)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return "The value cannot be empty.";
                }
            }
            if (MaxLength.HasValue)
            {
                if (value.Length > MaxLength.Value)
                {
                    return $"The value cannot be longer than {MaxLength.Value} characters.";
                }
            }
            if (MinLength.HasValue)
            {
                if (value.Length < MinLength.Value)
                {
                    return $"The value cannot be longer than {MinLength.Value} characters.";
                }
            }
            return null;
        }
    }
}