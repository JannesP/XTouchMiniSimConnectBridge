#pragma once

namespace BridgeProtocol {
	static const int PACKET_DATA_SIZE = 128;
	static const char* PUBLIC_DOWNLINK_CHANNEL = "jannesp.xtouchminisimconnectbridge.downlink";
	static const char* PUBLIC_UPLINK_CHANNEL = "jannesp.xtouchminisimconnectbridge.uplink";

	class BridgePacket {
	public:
		int id;
		char data[PACKET_DATA_SIZE];
		BridgePacket(char data[]);
	};

	enum ClientDataDefinitions {
		PacketDefinition = 5842,
	};

	enum ClientDataAreas {
		PublicDownlinkArea = 5842,
		PublicUplinkArea = 5843,
	};

	enum DataRequest {
		UplinkRequest = 5842,
		DownlinkRequest = 5843,
	};
}

