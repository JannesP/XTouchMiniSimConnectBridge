#pragma once

#include <MSFS/MSFS_WindowsTypes.h>
#include <SimConnect.h>

class BridgeServer
{
public:
	BridgeServer();

	const char *SIMCONNECT_APP_NAME = "DeviceSimConnectBridgeServerModule";
	static void CALLBACK SimConnectMessageCallback(SIMCONNECT_RECV *pData, DWORD cbData, void *pContext);

	void Start();
	void Shutdown();

private:
	HANDLE hSimConnect;

	void HandleSimConnectMessage(SIMCONNECT_RECV *pData, DWORD cbData);
	void HandleSimConnectReceiveClientData(SIMCONNECT_RECV_CLIENT_DATA *pData);
	void InitSimConnect();
};
