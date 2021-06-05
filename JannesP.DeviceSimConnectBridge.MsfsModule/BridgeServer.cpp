#include "BridgeServer.h"

// MSFS includes
#include <MSFS/Legacy/gauges.h>
#include <MSFS/MSFS.h>
#include <MSFS/MSFS_WindowsTypes.h>
#include <SimConnect.h>

#include <iostream>

#include "Global.h"
#include "ApiProtocol.h"

BridgeServer::BridgeServer()
{
	LOG("BridgeServer::BridgeServer()");
}

void CALLBACK BridgeServer::SimConnectMessageCallback(SIMCONNECT_RECV *pData, DWORD cbData, void *pContext)
{
	static_cast<BridgeServer *>(pContext)->HandleSimConnectMessage(pData, cbData);
}

void BridgeServer::Start()
{
	TLOG;
	// ignore call if simconnect is already open
	if (this->hSimConnect) return;
	this->InitSimConnect();
}

void BridgeServer::Shutdown()
{
	TLOG;
	// ignore call if simconnect is already closed
	if (!this->hSimConnect) return;

	SimConnect_Close(this->hSimConnect);
	this->hSimConnect = 0;
}

void BridgeServer::HandleSimConnectMessage(SIMCONNECT_RECV *pData, DWORD cbData)
{
	switch (pData->dwID) {
	case SIMCONNECT_RECV_ID_OPEN:
		DLOG("SIMCONNECT_RECV_ID_OPEN");
		break;
	case SIMCONNECT_RECV_ID_QUIT:
		DLOG("SIMCONNECT_RECV_ID_QUIT");
		break;
	case SIMCONNECT_RECV_ID_CLIENT_DATA:
		DLOG("SIMCONNECT_RECV_ID_CLIENT_DATA");
		this->HandleSimConnectReceiveClientData(static_cast<SIMCONNECT_RECV_CLIENT_DATA *>(pData));
		break;
	}
}

void BridgeServer::HandleSimConnectReceiveClientData(SIMCONNECT_RECV_CLIENT_DATA *pData)
{
	auto request = ApiProtocol::CreateClientRequest(pData);
	request->Respond(this->hSimConnect);
}

void BridgeServer::InitSimConnect()
{
	SimConnect_Open(&this->hSimConnect, SIMCONNECT_APP_NAME, 0, 0, 0, 0);

	ApiProtocol::SetupChannels(this->hSimConnect);

	SimConnect_CallDispatch(this->hSimConnect, &BridgeServer::SimConnectMessageCallback, this);
}