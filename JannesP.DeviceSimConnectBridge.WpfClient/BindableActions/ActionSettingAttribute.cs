using System;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions
{

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public abstract class ActionSettingAttribute : Attribute
    {
        public ActionSettingAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }

        public abstract string? ValidateValue(object? value);
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class StringActionSettingAttribute : ActionSettingAttribute
    {
        public StringActionSettingAttribute(string name, string description) : base(name, description) { }

        public virtual bool CanBeEmpty { get; set; } = false;
        public virtual int? MaxLength { get; set; } = null;
        public virtual int? MinLength { get; set; } = null;

        public override string? ValidateValue(object? value)
        {
            string? sValue = value as string;
            if (value != null && sValue == null)
            {
                throw new ArgumentException("This validation only works with string.");
            }            

            if (!CanBeEmpty)
            {
                if (string.IsNullOrWhiteSpace(sValue))
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

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class IntActionSettingAttribute : ActionSettingAttribute
    {
        public IntActionSettingAttribute(string name, string description) : base(name, description) { }

        public virtual int Min { get; set; }
        public virtual int Max { get; set; }
        public virtual bool CanBeNull { get; set; } = false;

        public override string? ValidateValue(object? value)
        {
            if (value != null)
            {
                if (value is int iValue)
                {
                    if (Min != 0 && iValue < Min)
                    {
                        return $"The value needs to be larger than or equal to {Min}";
                    }
                    if (Max != 0 && iValue > Max)
                    {
                        return $"The value needs to be smaller than or equal to {Max}";
                    }
                }
                else
                {
                    throw new ArgumentException("This method can only validate int.", nameof(value));
                }
            }
            else
            {
                if (!CanBeNull) return "This value needs to be set.";
            }
            return null;
        }
    }
}