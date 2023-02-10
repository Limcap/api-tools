using System;
using System.Net;

namespace StandardApiTools {
    public class StdApiHttpException: Exception, IProduceStdApiResult {

        public StdApiHttpException(HttpStatusCode status, string message=null, object details = null, Exception ex = null)
        : base(message, ex) {
            Result = new StdResult((int)status, message??status.ToString(), details);
        }

        public StdResult Result { get; }
    }
}
