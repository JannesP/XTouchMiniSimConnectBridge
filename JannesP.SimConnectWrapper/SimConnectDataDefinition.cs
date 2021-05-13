using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.SimConnectWrapper
{
    public class SimConnectDataDefinition
    {
        private static int _definitionIdCount = 0;

        /// <summary>
        /// This is a VERY uncomplete list of typically used Sim units.
        /// </summary>
        public static class SimConnectUnitName
        {
            public static string Bool = "Bool";
        } 

        
        public SimConnectDataDefinition(string dataName, string? unitName, SimConnectDataType dataType)
        {
            DefinitionId = Interlocked.Increment(ref _definitionIdCount);
            DataName = dataName;
            UnitName = unitName;
            DataType = dataType;
        }

        internal int DefinitionId { get; }
        public string DataName { get; }
        public string? UnitName { get; }
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
