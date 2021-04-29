using System;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions
{

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public abstract class ActionSettingAttribute : Attribute
    {
        public ActionSettingAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract string? ValidateValue(object value);
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class StringActionSettingAttribute : ActionSettingAttribute
    {
        public StringActionSettingAttribute(string name) : base(name) { }

        public virtual bool CanBeEmpty { get; set; } = true;
        public virtual int? MaxLength { get; set; } = null;
        public virtual int? MinLength { get; set; } = null;

        public override string? ValidateValue(object value)
        {
            string? sValue = value as string;

            if (!CanBeEmpty)
            {
                if (string.IsNullOrEmpty(sValue))
                {
                    return "The value cannot be empty.";
                }
            }
            if (sValue != null)
            {
                if (MaxLength.HasValue)
                {
                    if (sValue.Length > MaxLength.Value)
                    {
                        return $"The value cannot be longer than {MaxLength.Value} characters.";
                    }
                }
                if (MinLength.HasValue)
                {
                    if (sValue.Length < MinLength.Value)
                    {
                        return $"The value cannot be longer than {MinLength.Value} characters.";
                    }
                }
            }
            return null;
        }
    }
}