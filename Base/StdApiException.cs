using System;
using System.Net;

namespace StandardApiTools {

    public class StdApiException: Exception, IProduceStdApiResult {

        public StdApiException(Exception innerException, string message = null)
        : base(message ?? innerException.Message, innerException) {
            StatusCode = 500;
            Content = innerException.ToString();
        }




        public StdApiException(string message, object content = null)
        : base(message) {
            StatusCode = 500;
            Content = content;
        }




        public StdApiException(HttpStatusCode code, string message = null, object content = null)
        : base(message ?? code.ToString()) {
            StatusCode = (int)code;
            Content = content;
        }




        public readonly int StatusCode;
        public readonly object Content;




        public StdApiResult ToResult() => new StdApiResult(StatusCode, Message, Content);




        public void Throw() {
            throw this;
        }
    }
}
