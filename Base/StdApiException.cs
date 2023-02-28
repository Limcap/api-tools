using System;
using System.Collections.Generic;
using System.Net;

namespace StandardApiTools {

    public class StdApiException: StdApiExceptionBase, IAddInfo {

        protected StdApiException(WebException innerException, string message = null)
        : base(message, innerException) {
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        public StdApiException(Exception innerException, string message = null)
        : base(message ?? innerException.Message, innerException) {
            this.statusCode = 500;
            this.details = innerException.ToString();
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        public StdApiException(string message, object details = null)
        : base(message) {
            this.statusCode = 500;
            this.details = details;
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        public StdApiException(HttpStatusCode code, string message = null, object details = null)
        : base(message ?? code.ToString()) {
            this.statusCode = (int)code;
            this.details = details;
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        public new StdApiDataCollection Info { get => info as StdApiDataCollection; }




        IAddInfo IAddInfo.AddInfo(string key, object value) => this.AddInfo(key, value);




        public virtual StdApiException SetStatus(HttpStatusCode status) {
            statusCode = (int)status;
            return this;
        }




        public virtual StdApiException SetDetails(object details) {
            this.details = details;
            return this;
        }
    }
}
