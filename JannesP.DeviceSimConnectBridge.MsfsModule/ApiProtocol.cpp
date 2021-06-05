#include "ApiProtocol.h"

#include <MSFS/MSFS_WindowsTypes.h>
#include <SimConnect.h>

#include "Global.h"

#include "ApiRequestReadLVar.h"
#include "ApiRequestExecuteCalculatorCode.h"

namespace ApiProtocol {
	ApiRequest *CreateClientRequest(SIMCONNECT_RECV_CLIENT_DATA *pData)
	{
		auto requestType = static_cast<ClientRequests>(pData->dwDefineID);
		switch (requestType) {
		case ReadLVar:
			return new ApiRequestReadLVar(pData);
			break;
		case FireHEvent:
			return new ApiRequestExecuteCalculatorCode(pData);
			break;
		default:
			DLOG("Unexpected defineID: " << std::to_string(requestType));
			break;
		}
	}

	void SetupChannels(HANDLE hSimConnect)
	{
		ApiRequestReadLVar::SetupChannels(hSimConnect);
		ApiRequestExecuteCalculatorCode::SetupChannels(hSimConnect);
	}
}