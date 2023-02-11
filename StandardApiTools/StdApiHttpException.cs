using System;
using System.Net;

namespace StandardApiTools {

    public class StdApiException: Exception, IProduceStdApiResult {

        public StdApiException(string message,
            object details = null,
            Exception innerException = null)
        : base(message, innerException) {
            Result = new StdApiResult(500, message, details);
        }




        public StdApiException(HttpStatusCode code, string message = null,object details = null, Exception innerException = null)
        : base(message ?? code.ToString(), innerException) {
            Result = new StdApiResult((int)code, message ?? code.ToString(), details);
        }




        public StdApiResult Result { get; }
    }
}
