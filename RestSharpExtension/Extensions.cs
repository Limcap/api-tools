using Microsoft.AspNetCore.Hosting.Server;
using RestSharp;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace StandardApiTools.RestSharp {
    public static class Extensions {

        public static async Task<StdApiResponse> GetStdApiResponseAsync(this IRestClient client, IRestRequest req) {
            var resp = await client.ExecuteAsync(req);
            return CreateResponse(client, resp);
        }



        public static StdApiResponse GetStdApiResponse(this IRestClient client, IRestRequest req) {
            var resp = client.Execute(req);
            return CreateResponse(client, resp);
        }



        private static StdApiResponse CreateResponse(IRestClient client, IRestResponse resp) {
            var blueprint = new StdApiResponse.Blueprint() {
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
                blueprint.CommStatusCode = (StdApiResponse.CommunicationStatus)(int)resp.StatusCode;
                blueprint.HttpStatusCode = null;
            }
            return new StdApiResponse(blueprint);
        }



        public static StdApiResponse.CommunicationStatus ToCommStatus(this ResponseStatus s) {
            switch (s) {
                case ResponseStatus.None: return StdApiResponse.CommunicationStatus.Pending;
                case ResponseStatus.Completed: return StdApiResponse.CommunicationStatus.Success;
                case ResponseStatus.TimedOut: return StdApiResponse.CommunicationStatus.Timeout;
                case ResponseStatus.Aborted: return StdApiResponse.CommunicationStatus.Aborted;
                case ResponseStatus.Error: return StdApiResponse.CommunicationStatus.UnknownError;
                default: return StdApiResponse.CommunicationStatus.UnknownError;
            }
        }
    }
}
