#pragma once

#include <MSFS/MSFS_WindowsTypes.h>
#include <SimConnect.h>

#include <string>

#include "ApiProtocol.h"

namespace ApiProtocol {
	struct ApiResponseReadLVarData {
		int requestId;
		double value;
	};
	struct ApiRequestReadLVarData {
		int requestId;
		char lvarName[128];
	};

	class ApiRequestReadLVar : public TypedApiRequest<ApiRequestReadLVarData, ApiResponseReadLVarData> {
	public:
		const static ClientRequests DefinitionId = ClientRequests::ReadLVar;
		const static DataAreaDefinition DataAreaModuleInput;
		const static DataAreaDefinition DataAreaModuleOutput;
		static void SetupChannels(HANDLE hSimConnect);
		ApiRequestReadLVar(SIMCONNECT_RECV_CLIENT_DATA *pData);
		void Respond(HANDLE hSimConnect) override;
	private:
		std::string lvarName;
	};
}