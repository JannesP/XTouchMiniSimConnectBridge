﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device.XTouchMini;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.DataSources;
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
                    DeviceType = "behringer_xtouch_mini",
                    DeviceId = null,
                    FriendlyName = "Behringer X-Touch Mini",
                    Bindings = new List<ActionBinding>()
                    {
                        new EncoderActionBinding()
                        {
                            DeviceControlId = 0x10,
                            IgnoreSpeed = true,
                            TurnClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "COM_RADIO_WHOLE_INC",
                            },
                            TurnAntiClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "COM_RADIO_WHOLE_DEC",
                            },
                        },
                        new EncoderActionBinding()
                        {
                            DeviceControlId = 0x11,
                            IgnoreSpeed = true,
                            TurnClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "COM_RADIO_FRACT_INC",
                            },
                            TurnAntiClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "COM_RADIO_FRACT_DEC",
                            },
                        },
                        new EncoderActionBinding()
                        {
                            DeviceControlId = 0x14,
                            TurnClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "AP_VS_VAR_INC",
                            },
                            TurnAntiClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "AP_VS_VAR_DEC",
                            },
                        },
                        new EncoderActionBinding()
                        {
                            DeviceControlId = 0x15,
                            TurnClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "AP_SPD_VAR_INC",
                            },
                            TurnAntiClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "AP_SPD_VAR_DEC",
                            },
                        },
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
                        new EncoderActionBinding()
                        {
                            DeviceControlId = 0x17,
                            TurnClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "AP_ALT_VAR_INC",
                            },
                            TurnAntiClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "AP_ALT_VAR_DEC",
                            },
                        },
                        new ButtonActionBinding()
                        {
                            DeviceControlId = 0x26,
                            TriggerOnRelease = false,
                            ButtonPressed = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "HEADING_BUG_SET",
                            },
                        },
                        new ButtonActionBinding()
                        {
                            DeviceControlId = 0x54,
                            TriggerOnRelease = false,
                            ButtonPressed = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "AP_MASTER",
                            },
                        },
                        new LedActionBinding()
                        {
                            DataSource = new SimVarBoolDataSource()
                            {
                                Interval = 1000,
                                SimVarName = "AUTOPILOT MASTER",
                            },
                            DeviceControlId = 0x54,
                        },
                    }
                }
            }
        };
    }

    public class DeviceBindingConfiguration
    {
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
        [MemberNotNull(nameof(DeviceType), nameof(FriendlyName))]
        public void ThrowIfNotComplete()
        {
            if (DeviceType == null) throw new Exception($"{nameof(DeviceType)} is null.");
            if (FriendlyName == null) throw new Exception($"{nameof(FriendlyName)} is null.");
        }
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

        public string? DeviceType { get; set; }
        public string? DeviceId { get; set; }
        public string? FriendlyName { get; set; }
        public List<ActionBinding> Bindings { get; set; } = new List<ActionBinding>();
    }
}
