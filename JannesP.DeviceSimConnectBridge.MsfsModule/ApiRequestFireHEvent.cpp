#include "ApiRequestFireHEvent.h"

#include "Global.h"

namespace ApiProtocol {
	const DataAreaDefinition ApiRequestFireHEvent::DataAreaModuleInput = { GetDataSize(), "jannesp.device_simconnect_bridge.module_input.fire_hevent", DefinitionId };
	const DataAreaDefinition ApiRequestFireHEvent::DataAreaModuleOutput = { GetResponseSize(), "jannesp.device_simconnect_bridge.module_output.fire_hevent", DefinitionId + 100 };

	void ApiRequestFireHEvent::SetupChannels(HANDLE hSimConnect) {
		SimConnect_AddToClientDataDefinition(hSimConnect, ApiRequestFireHEvent::DefinitionId, 0, ApiRequestFireHEvent::GetDataSize());
		SimConnect_AddToClientDataDefinition(hSimConnect, ApiRequestFireHEvent::DefinitionId + 100, 0, ApiRequestFireHEvent::GetResponseSize());
		SimConnect_MapClientDataNameToID(hSimConnect, ApiRequestFireHEvent::DataAreaModuleInput.name, ApiRequestFireHEvent::DataAreaModuleInput.id);
		SimConnect_MapClientDataNameToID(hSimConnect, ApiRequestFireHEvent::DataAreaModuleOutput.name, ApiRequestFireHEvent::DataAreaModuleOutput.id);
		SimConnect_CreateClientData(hSimConnect, ApiRequestFireHEvent::DataAreaModuleInput.id, ApiRequestFireHEvent::DataAreaModuleInput.size, SIMCONNECT_CREATE_CLIENT_DATA_FLAG_DEFAULT);
		SimConnect_CreateClientData(hSimConnect, ApiRequestFireHEvent::DataAreaModuleOutput.id, ApiRequestFireHEvent::DataAreaModuleOutput.size, SIMCONNECT_CREATE_CLIENT_DATA_FLAG_READ_ONLY);
		SimConnect_RequestClientData(hSimConnect, ApiRequestFireHEvent::DataAreaModuleInput.id, ApiRequestFireHEvent::DefinitionId, ApiRequestFireHEvent::DefinitionId, SIMCONNECT_CLIENT_DATA_PERIOD_ON_SET);
	}

	ApiRequestFireHEvent::ApiRequestFireHEvent(SIMCONNECT_RECV_CLIENT_DATA *pData)
	{
		ApiRequestFireHEventData *reqData = reinterpret_cast<ApiRequestFireHEventData *>(&pData->dwData);
		this->hEventName = reqData->hEventName;
	}
	void ApiRequestFireHEvent::Respond(HANDLE hSimConnect)
	{
		LOG("ApiRequestFireHEvent::Respond(HANDLE hSimConnect): " << this->hEventName.c_str());
		ApiResponseFireHEventData *response = new ApiResponseFireHEventData();
		response->requestId = 420;
		SimConnect_SetClientData(hSimConnect, DataAreaModuleOutput.id, DefinitionId + 100, SIMCONNECT_CLIENT_DATA_SET_FLAG_DEFAULT, 0, DataAreaModuleOutput.size, response);
		LOG("ApiRequestReadLVar::Respond(HANDLE hSimConnect): Responded!");
	}
}