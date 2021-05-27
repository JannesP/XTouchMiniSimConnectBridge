#pragma once

#include <MSFS/MSFS_WindowsTypes.h>
#include <SimConnect.h>

#include <string>

namespace ApiProtocol {
	struct DataAreaDefinition {
		const DWORD size;
		const char *name;
		const int id;
	};

	enum ClientRequests {
		ReadLVar = 1,
		FireHEvent = 2,
	};

	class ApiRequest {
	public:
		virtual void Respond(HANDLE hSimConnect) = 0;
	};

	template <typename TClientData>
	class TypedApiRequest : public ApiRequest {
	public:
		static int GetDataSize() { return sizeof(TClientData); };
	};

	template <typename TClientData, typename TResponseData>
	class TypedApiRequestWithResponse : public TypedApiRequest<TClientData> {
	public:
		static int GetResponseSize() { return sizeof(TResponseData); };
	};

	ApiRequest *CreateClientRequest(SIMCONNECT_RECV_CLIENT_DATA *pData);
	void SetupChannels(HANDLE hSimConnect);
}
