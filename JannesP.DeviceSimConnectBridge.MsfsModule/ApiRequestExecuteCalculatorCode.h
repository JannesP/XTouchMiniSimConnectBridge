#pragma once

#include <MSFS/MSFS_WindowsTypes.h>
#include <SimConnect.h>

#include <string>

#include "ApiProtocol.h"

namespace ApiProtocol {
	struct ApiRequestExecuteCalculatorCodeData {
		int requestId;
		char hEventName[128];
	};
	class ApiRequestExecuteCalculatorCode : public TypedApiRequest<ApiRequestExecuteCalculatorCodeData> {
	public:
		const static ClientRequests DefinitionId = ClientRequests::FireHEvent;
		const static DataAreaDefinition DataAreaModuleInput;
		static void SetupChannels(HANDLE hSimConnect);
		ApiRequestExecuteCalculatorCode(SIMCONNECT_RECV_CLIENT_DATA *pData);
		void Respond(HANDLE hSimConnect) override;
	private:
		std::string _hEventName;
	};
}