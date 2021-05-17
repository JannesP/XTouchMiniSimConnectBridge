#pragma once

#define LOG(MSG) std::cout << "[DeviceSimConnectBridgeServer] " << MSG << std::endl

namespace DeviceSimConnectBridge {
	class BridgeServer {
	public:
		BridgeServer();
		void Start();
		void Shutdown();
	};
}