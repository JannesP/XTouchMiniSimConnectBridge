using System;
using System.Collections.Generic;
using System.Text;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.Client
{
    internal class MsfsModuleChannelDefinition
    {
        public MsfsModuleChannelDefinition(string name, uint id, uint size)
        {
            Id = id;
            Name = name;
            Size = size;
        }

        public uint Id { get; }
        public string Name { get; }
        public uint Size { get; }
    }
}