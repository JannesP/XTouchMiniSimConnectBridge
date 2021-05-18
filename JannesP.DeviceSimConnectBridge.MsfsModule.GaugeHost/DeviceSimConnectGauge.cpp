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

// ------------------------
// Callbacks

DeviceSimConnectBridge::BridgeServer* Server;

void StartServer() {
	if (Server == NULL) {
		Server = new DeviceSimConnectBridge::BridgeServer();
	}
	Server->Start();
}

void ShutdownServer() {
	if (Server != NULL) {
		Server->Shutdown();
	}
}

extern "C" {
	MSFS_CALLBACK bool DeviceSimConnectGauge_gauge_callback(FsContext ctx, int service_id, void* pData)
	{
		switch (service_id)
		{
		case PANEL_SERVICE_PRE_INSTALL:
		{
			LOG("DeviceSimConnectGauge_gauge_callback(PANEL_SERVICE_PRE_INSTALL)");
			StartServer();
			return true;
		}
		break;

		case PANEL_SERVICE_PRE_KILL:
		{
			LOG("DeviceSimConnectGauge_gauge_callback(PANEL_SERVICE_PRE_KILL)");
			ShutdownServer();
			return true;
		}
		break;
		}
		return false;
	}
}