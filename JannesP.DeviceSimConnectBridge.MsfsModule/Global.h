#pragma once

#include <iostream>

#define LOG(MSG) std::cout << "[DeviceSimConnectBridgeModule] " << MSG << std::endl
#ifdef _DEBUG
#define DLOG(MSG) LOG(MSG)
#define TLOG LOG("[DeviceSimConnectBridgeModule] " << __func__)
#else // DEBUG
#define DLOG(MSG)
#define TLOG
#endif
