using System.Net;
using System.Threading.Tasks;

namespace StandardApiTools
{
    public static class StdApiResponseExtensions
    {
        public static Task<StdApiResponse> GetStdApiResponseAsync(this HttpWebRequest req)
        {
            return StdApiResponse.FromAsync(req);
        }




        public static StdApiResponse GetStdApiResponse(this HttpWebRequest req)
        {
            return StdApiResponse.From(req);
        }




        public static StdApiResponse.CommunicationStatus ToCommStatus(this WebExceptionStatus wss)
        {
            return (StdApiResponse.CommunicationStatus)(int)wss;
        }
    }
}
