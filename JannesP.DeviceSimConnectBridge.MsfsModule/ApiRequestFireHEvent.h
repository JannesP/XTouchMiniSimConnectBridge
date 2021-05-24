#pragma once

#include <MSFS/MSFS_WindowsTypes.h>
#include <SimConnect.h>

#include <string>

#include "ApiProtocol.h"

namespace ApiProtocol {
	struct ApiResponseFireHEventData {
		int requestId;
	};
	struct ApiRequestFireHEventData {
		int requestId;
		char hEventName[128];
	};
	class ApiRequestFireHEvent : public TypedApiRequest<ApiRequestFireHEventData, ApiResponseFireHEventData> {
	public:
		const static ClientRequests DefinitionId = ClientRequests::FireHEvent;
		const static DataAreaDefinition DataAreaModuleInput;
		const static DataAreaDefinition DataAreaModuleOutput;
		static void SetupChannels(HANDLE hSimConnect);
		ApiRequestFireHEvent(SIMCONNECT_RECV_CLIENT_DATA *pData);
		void Respond(HANDLE hSimConnect) override;
	private:
		std::string hEventName;
	};
}