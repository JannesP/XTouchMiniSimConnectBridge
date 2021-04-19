// XTouchMiniSimconnectBridgeModule.cpp
#include "XTouchMiniSimconnectBridgeModule.h"

// Microsoft Flight Simulator includes.
#include <MSFS/Legacy/gauges.h>
#include <MSFS/MSFS.h>
#include <MSFS/MSFS_WindowsTypes.h>
#include <SimConnect.h>

// Standard Template Library includes.
#include <iostream>
#include <string>

#include "BridgeProtocol.h"

using namespace BridgeProtocol;

const char* CoutPrefix = "[XTouchMiniSimConnectBridgeModule] ";
HANDLE simconnect = 0;

void CALLBACK HandleSimconnectMessage(SIMCONNECT_RECV* pData, DWORD cbData, void* pContext) {
    switch (pData->dwID) {
        case SIMCONNECT_RECV_ID_CLIENT_DATA: {
            auto received_data = static_cast<SIMCONNECT_RECV_CLIENT_DATA*>(pData);
            auto packet = (BridgePacket*)&received_data->dwData;

            // TODO: better opcode decoding.
            if (packet->data[0] != 'x') return;

            std::string code = (char*)(packet->data + 1);
            std::cout << CoutPrefix << " [" << packet->id << ":x] " << code << std::endl;
            execute_calculator_code(code.c_str(), 0, 0, 0);

            // Reply with the same exact same packet (for now).
            SimConnect_SetClientData(simconnect, PublicDownlinkArea, PacketDefinition, 0, 0,
                sizeof(BridgePacket), packet);
            break;
        }
    }
}

extern "C" MSFS_CALLBACK void module_init() {
    // This will (hopefully) be visibile in the Microsoft Flight Simulator console.
    std::cout << CoutPrefix << "Module version " << ModuleVersion << " initialising." << std::endl;

    // TODO: implement simconnect error handling.
    if (!SUCCEEDED(SimConnect_Open(&simconnect, SimConnectClientName, 0, 0, 0, 0))) {
        std::cout << CoutPrefix << "Failed to connect to SimConnect. Module will be Inop." << std::endl;
        return;
    }

    // Define a custom ClientDataDefinition for jetbridge::Packet.
    SimConnect_AddToClientDataDefinition(simconnect, PacketDefinition, 0, sizeof(BridgePacket));

    // Map the public downlink and uplink channels with own ids for them (see CleitnDataAreas enum).
    SimConnect_MapClientDataNameToID(simconnect, PUBLIC_DOWNLINK_CHANNEL, PublicDownlinkArea);
    SimConnect_MapClientDataNameToID(simconnect, PUBLIC_UPLINK_CHANNEL, PublicUplinkArea);

    // Create the public downlink and uplink channels (which are actually ClientData areas).
    SimConnect_CreateClientData(simconnect, PublicDownlinkArea, sizeof(BridgePacket), 0);
    SimConnect_CreateClientData(simconnect, PublicUplinkArea, sizeof(BridgePacket), 0);

    // Request to be notified (via Simconnect Dispatch) for any changes to the public uplink channel.
    SimConnect_RequestClientData(simconnect, PublicUplinkArea, UplinkRequest,
        PacketDefinition, SIMCONNECT_CLIENT_DATA_PERIOD_ON_SET,
        SIMCONNECT_CLIENT_DATA_REQUEST_FLAG_CHANGED);

    SimConnect_CallDispatch(simconnect, HandleSimconnectMessage, 0);
}

extern "C" MODULE_EXPORT void test(void)
{
    if (!simconnect) return;
    SimConnect_Close(simconnect);
}
