﻿// DeviceSimConnectBridgeModule.cpp
#include "DeviceSimConnectBridgeModule.h"

// Microsoft Flight Simulator includes.
#include <MSFS/Legacy/gauges.h>
#include <MSFS/MSFS.h>
#include <MSFS/MSFS_WindowsTypes.h>

// Standard Template Library includes.
#include <iostream>

#include "Global.h"
#include "BridgeServer.h"

BridgeServer *Server;

void StartServer() {
	TLOG;
	if (Server == NULL) {
		Server = new BridgeServer();
	}
	Server->Start();
}

void ShutdownServer() {
	TLOG;
	if (Server != NULL) {
		Server->Shutdown();
	}
}

extern "C" MSFS_CALLBACK void module_init() {
	// This will (hopefully) be visibile in the Microsoft Flight Simulator console.
	TLOG;
	StartServer();
}

extern "C" MSFS_CALLBACK void module_deinit()
{
	TLOG;
	ShutdownServer();
}