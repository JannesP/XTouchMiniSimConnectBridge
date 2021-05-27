#include "ApiRequestReadLVar.h"

#include <MSFS/Legacy/gauges.h>
#include <MSFS/MSFS.h>
#include <MSFS/MSFS_WindowsTypes.h>

#include "Global.h"

namespace ApiProtocol {
	const DataAreaDefinition ApiRequestReadLVar::DataAreaModuleInput = { GetDataSize(), "jannesp.device_simconnect_bridge.module_input.read_lvar", DefinitionId };
	const DataAreaDefinition ApiRequestReadLVar::DataAreaModuleOutput = { GetResponseSize(), "jannesp.device_simconnect_bridge.module_output.read_lvar", DefinitionId + 100 };

	void ApiRequestReadLVar::SetupChannels(HANDLE hSimConnect) {
		SimConnect_AddToClientDataDefinition(hSimConnect, ApiRequestReadLVar::DefinitionId, 0, ApiRequestReadLVar::GetDataSize());
		SimConnect_AddToClientDataDefinition(hSimConnect, ApiRequestReadLVar::DefinitionId + 100, 0, ApiRequestReadLVar::GetResponseSize());
		SimConnect_MapClientDataNameToID(hSimConnect, ApiRequestReadLVar::DataAreaModuleInput.name, ApiRequestReadLVar::DataAreaModuleInput.id);
		SimConnect_MapClientDataNameToID(hSimConnect, ApiRequestReadLVar::DataAreaModuleOutput.name, ApiRequestReadLVar::DataAreaModuleOutput.id);
		SimConnect_CreateClientData(hSimConnect, ApiRequestReadLVar::DataAreaModuleInput.id, ApiRequestReadLVar::DataAreaModuleInput.size, 0);
		SimConnect_CreateClientData(hSimConnect, ApiRequestReadLVar::DataAreaModuleOutput.id, ApiRequestReadLVar::DataAreaModuleOutput.size, SIMCONNECT_CREATE_CLIENT_DATA_FLAG_READ_ONLY);
		SimConnect_RequestClientData(hSimConnect, ApiRequestReadLVar::DataAreaModuleInput.id, ApiRequestReadLVar::DefinitionId, ApiRequestReadLVar::DefinitionId, SIMCONNECT_CLIENT_DATA_PERIOD_ON_SET);
	}

	ApiRequestReadLVar::ApiRequestReadLVar(SIMCONNECT_RECV_CLIENT_DATA *pData)
	{
		ApiRequestReadLVarData *reqData = reinterpret_cast<ApiRequestReadLVarData *>(&pData->dwData);
		_lvarName = reqData->lvarName;
		_requestId = reqData->requestId;
	}

	void ApiRequestReadLVar::Respond(HANDLE hSimConnect)
	{
		LOG("ApiRequestReadLVar::Respond(HANDLE hSimConnect): " << _lvarName.c_str());
		//get variable by name
		int lvar = check_named_variable(_lvarName.c_str());
		if (lvar == -1) {
			LOG("ApiRequestReadLVar::Respond(HANDLE hSimConnect): Requested LVar not found.");
			return;
		}
		double value = get_named_variable_value(lvar);

		ApiResponseReadLVarData *response = new ApiResponseReadLVarData();
		response->requestId = _requestId;
		response->value = value;
		SimConnect_SetClientData(hSimConnect, DataAreaModuleOutput.id, DefinitionId + 100, SIMCONNECT_CLIENT_DATA_SET_FLAG_DEFAULT, 0, DataAreaModuleOutput.size, response);
		LOG("ApiRequestReadLVar::Respond(HANDLE hSimConnect): Responded!");
	}
}