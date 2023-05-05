using RestSharp;
using System.Linq;
using System.Threading.Tasks;

namespace Limcap.ApiTools.RestSharp {
	public static class Extensions {

		public static async Task<StdResponse> GetApiResponseAsync( this IRestClient client, IRestRequest req ) {
			var resp = await client.ExecuteAsync(req);
			return CreateResponse(client, resp);
		}



		public static StdResponse GetApiResponse( this IRestClient client, IRestRequest req ) {
			var resp = client.Execute(req);
			return CreateResponse(client, resp);
		}



		private static StdResponse CreateResponse( IRestClient client, IRestResponse resp ) {
			var blueprint = new StdResponse.Blueprint() {
				Exception = resp.ErrorException,
				CommMessage = resp.ErrorMessage,
				CharacterSet = null,
				ContentBytes = resp.RawBytes,
				//ContentAsString = resp.Content,
				ContentEncoding = resp.ContentEncoding,
				ContentType = resp.ContentType,
				Headers = resp.Headers.ToDictionary(p => p.Name, p => p.Value.ToString()),
				Method = resp.Request.Method.ToString(),
				ProtocolVersion = resp.ProtocolVersion,
				Server = resp.Server,
				RequestUri = client.BaseUrl,
				LastModified = null,
				IsFromCache = false,
				ContentLength = resp.ContentLength,
			};
			if ((int)resp.StatusCode > 99) {
				blueprint.CommStatusCode = resp.ResponseStatus.ToCommStatus();
				blueprint.HttpStatusCode = resp.StatusCode;
			}
			else {
				blueprint.CommStatusCode = (CommunicationStatus)(int)resp.StatusCode;
				blueprint.HttpStatusCode = null;
			}
			return new StdResponse(blueprint);
		}



		public static CommunicationStatus ToCommStatus( this ResponseStatus s ) {
			switch (s) {
				case ResponseStatus.None: return CommunicationStatus.Pending;
				case ResponseStatus.Completed: return CommunicationStatus.Success;
				case ResponseStatus.TimedOut: return CommunicationStatus.Timeout;
				case ResponseStatus.Aborted: return CommunicationStatus.Aborted;
				case ResponseStatus.Error: return CommunicationStatus.UnknownError;
				default: return CommunicationStatus.UnknownError;
			}
		}
	}
}
