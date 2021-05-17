// Copyright (c) Asobo Studio, All rights reserved. www.asobostudio.com

#include <MSFS\MSFS.h>
#include <MSFS\Legacy\gauges.h>

#include <iostream>
#include <stdio.h>
#include <string.h>

#include "../JannesP.DeviceSimConnectBridge.MsfsModule.Implementation/DeviceSimConnectBridgeServer.h"

#ifdef _MSC_VER
#define snprintf _snprintf_s
#elif !defined(__MINGW32__)
#include <iconv.h>
#endif

DeviceSimConnectBridge::BridgeServer* Server;

// ------------------------
// Callbacks
extern "C" {
	MSFS_CALLBACK bool DeviceSimConnectGauge_gauge_callback(FsContext ctx, int service_id, void* pData)
	{
		switch (service_id)
		{
		case PANEL_SERVICE_PRE_INSTALL:
		{
			LOG("DeviceSimConnectGauge_gauge_callback(PANEL_SERVICE_PRE_INSTALL)");
			if (Server == NULL) {
				Server = new DeviceSimConnectBridge::BridgeServer();
			}
			Server->Start();
			return true;
		}
		break;

		case PANEL_SERVICE_PRE_KILL:
		{
			LOG("DeviceSimConnectGauge_gauge_callback(PANEL_SERVICE_PRE_KILL)");
			if (Server != NULL) {
				Server->Shutdown();
			}
			return true;
		}
		break;
		}
		return false;
	}
}