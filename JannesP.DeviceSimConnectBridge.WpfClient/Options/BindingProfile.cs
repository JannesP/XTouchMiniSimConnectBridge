using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device.XTouchMini;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.SimConnectActions;
using Newtonsoft.Json;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Options
{
    public class BindingProfile
    {
        private string? _fileName;

        [JsonIgnore]
        public string? FileName 
        { 
            get => _fileName;
            set 
            {
                if (string.IsNullOrWhiteSpace(value) ||
                    value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    throw new ArgumentException("The given FileName is not a valid filename.", nameof(value));
                }
                _fileName = value;
            } 
        }

        public Guid? UniqueId { get; set; }
        public string? Name { get; set; }

        public List<DeviceBindingConfiguration>? BindingConfigurations { get; set; }

        public static BindingProfile CreateDefaultProfile() => new()
        {
            UniqueId = Guid.NewGuid(),
            Name = Constants.DefaultProfileName,
            FileName = Constants.DefaultProfileName.ToLowerInvariant(),
            BindingConfigurations = new List<DeviceBindingConfiguration>() {
                new DeviceBindingConfiguration
                {
                    TechnicalDeviceIdentifier = "behringer_xtouch_mini",
                    Bindings = new List<ActionBinding>()
                    {
                        new EncoderActionBinding()
                        {
                            DeviceControlId = 0x16,
                            TurnClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "HEADING_BUG_INC",
                            },
                            TurnAntiClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "HEADING_BUG_DEC",
                            },
                        },
                    }
                }
            }
        };
    }

    public class DeviceBindingConfiguration
    {
        public string? TechnicalDeviceIdentifier { get; set; }
        public List<ActionBinding> Bindings { get; set; } = new List<ActionBinding>();
    }
}
