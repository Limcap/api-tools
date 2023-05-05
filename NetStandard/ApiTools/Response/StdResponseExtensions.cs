using System.Net;
using System.Threading.Tasks;

namespace Limcap.ApiTools {
	public static class StdResponseExtensions {
		public static Task<StdResponse> GetStdResponseAsync( this HttpWebRequest req ) {
			return StdResponse.FromAsync(req);
		}




		public static StdResponse GetStdResponse( this HttpWebRequest req ) {
			return StdResponse.From(req);
		}




		public static CommunicationStatus ToCommStatus( this WebExceptionStatus wss ) {
			return (CommunicationStatus)(int)wss;
		}



		public static CommunicationStatus ToCommStatus( HttpStatusCode code ) {
			return (CommunicationStatus)(int)code;
		}
	}
}
