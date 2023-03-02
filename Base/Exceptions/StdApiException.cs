using System;
using System.Collections.Generic;
using System.Net;

namespace StandardApiTools {

    public class StdApiException: StdApiExceptionBase, IAddInfo {

        protected StdApiException(WebException innerException, string message = null)
        : base(message, innerException) {
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        protected StdApiException(Exception sourceException, string message = null)
        : base(message ?? Status500DefaultMessage, sourceException) {
            this.statusCode = 500;
            this.details = sourceException.ToString();
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




        public static StdApiException CreateFrom(Exception ex, string message = null) {
            ex = ex.Deaggregate();
            if (ex is StdApiException ex2) {
                if(message != null && !ex2.MessageParts.Contains(message)) ex2.InsertMessage(message);
                return ex2;
            }
            else return new StdApiException(ex, message); 
        }




        private static string Status500DefaultMessage = "Ocorreu um erro não identificado durante o processamento.";
    }
}
