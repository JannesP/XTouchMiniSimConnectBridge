#pragma once

#define LOG(MSG) std::cout << "[DeviceSimConnectBridgeModule] " << MSG << std::endl
#ifdef _DEBUG
#define DLOG std::cout << "[DeviceSimConnectBridgeModule] " << __func__ << std::endl
#else // DEBUG
#define DLOG
#endif
