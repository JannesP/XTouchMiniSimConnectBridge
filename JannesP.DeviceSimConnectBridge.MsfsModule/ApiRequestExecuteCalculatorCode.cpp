#include "ApiRequestExecuteCalculatorCode.h"

#include <MSFS/Legacy/gauges.h>
#include <MSFS/MSFS.h>
#include <MSFS/MSFS_WindowsTypes.h>

#include "Global.h"

namespace ApiProtocol {
	const DataAreaDefinition ApiRequestExecuteCalculatorCode::DataAreaModuleInput = { GetDataSize(), "jannesp.device_simconnect_bridge.module_input.fire_hevent", DefinitionId };

	void ApiRequestExecuteCalculatorCode::SetupChannels(HANDLE hSimConnect) {
		SimConnect_AddToClientDataDefinition(hSimConnect, ApiRequestExecuteCalculatorCode::DefinitionId, 0, ApiRequestExecuteCalculatorCode::GetDataSize());
		SimConnect_MapClientDataNameToID(hSimConnect, ApiRequestExecuteCalculatorCode::DataAreaModuleInput.name, ApiRequestExecuteCalculatorCode::DataAreaModuleInput.id);
		SimConnect_CreateClientData(hSimConnect, ApiRequestExecuteCalculatorCode::DataAreaModuleInput.id, ApiRequestExecuteCalculatorCode::DataAreaModuleInput.size, SIMCONNECT_CREATE_CLIENT_DATA_FLAG_DEFAULT);
		SimConnect_RequestClientData(hSimConnect, ApiRequestExecuteCalculatorCode::DataAreaModuleInput.id, ApiRequestExecuteCalculatorCode::DefinitionId, ApiRequestExecuteCalculatorCode::DefinitionId, SIMCONNECT_CLIENT_DATA_PERIOD_ON_SET);
	}

	ApiRequestExecuteCalculatorCode::ApiRequestExecuteCalculatorCode(SIMCONNECT_RECV_CLIENT_DATA *pData)
	{
		ApiRequestExecuteCalculatorCodeData *reqData = reinterpret_cast<ApiRequestExecuteCalculatorCodeData *>(&pData->dwData);
		this->_hEventName = reqData->hEventName;
	}
	void ApiRequestExecuteCalculatorCode::Respond(HANDLE hSimConnect)
	{
		LOG("ApiRequestFireHEvent::Respond(HANDLE hSimConnect): " << this->_hEventName.c_str());
		BOOL res = execute_calculator_code(this->_hEventName.c_str(), nullptr, nullptr, nullptr);
		if (res == 0) {
			LOG("ApiRequestReadLVar::Respond(HANDLE hSimConnect): Execution failed.");
		}
		else {
			LOG("ApiRequestReadLVar::Respond(HANDLE hSimConnect): Execution successful.");
		}
	}
}