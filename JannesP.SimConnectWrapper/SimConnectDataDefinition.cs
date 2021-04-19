using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.SimConnectWrapper
{
    public class SimConnectDataDefinition
    {
        public SimConnectDataDefinition(int definitionId, string dataName, string unitName, SimConnectDataType dataType) 
        {
            DefinitionId = definitionId;
            DataName = dataName;
            UnitName = unitName;
            DataType = dataType;
        }

        public int DefinitionId { get; }
        public string DataName { get; }
        public string UnitName { get; }
        public SimConnectDataType DataType { get; }

        internal SIMCONNECT_DATATYPE SimConnectDataType => (SIMCONNECT_DATATYPE)DataType;

        public override bool Equals(object? obj) => obj is SimConnectDataDefinition definition && 
            DefinitionId == definition.DefinitionId && 
            DataName == definition.DataName && 
            UnitName == definition.UnitName &&
            DataType == definition.DataType;
        public override int GetHashCode() => HashCode.Combine(DefinitionId, DataName, UnitName, DataType);
    }
}
