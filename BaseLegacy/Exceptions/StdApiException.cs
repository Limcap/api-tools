using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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
        : this(500, message, details) { }




        public StdApiException(HttpStatusCode code, string message = null, object details = null)
        : this((int)code, message ?? code.ToString(), details) { }




        public StdApiException(StdApiResponse.CommunicationStatus code, string message = null, object details = null)
        : this((int)code, message ?? code.ToString(), details) { }




        public StdApiException(int code, string message = null, object details = null)
        : base(message) {
            this.statusCode = code;
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




        public StdApiErrorResult ToResult(bool includeStackTraceInfo) {
            if (includeStackTraceInfo) Info.Add("StackTrace", StackTrace);
            return base.ToResult();
        }




        public virtual StdApiException SourceException() {
            //if (StatusCode != 424 || details == null) return this;
            if (details == null) return this;
            JObject j;
            try {
                string str = details as string ?? JsonConvert.SerializeObject(details);
                j = JObject.Parse(str);
            }
            catch { return this; }
            if (!j.TryGetValue("status", out _) || !j.TryGetValue("message", out _) || !j.TryGetValue("details", out _))
                return this;
            var s = j["status"].ToObject<int>();
            var m = j["message"].ToString();
            var d = j["details"].ToString();
            var ex = new StdApiException(s, m, d);
            if (j.TryGetValue("info", out _) && j["info"].Type == JTokenType.Array) {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(j["info"].ToString());
                ex.info = new StdApiDataCollection(dic);
            }
            return ex.SourceException();
            //string detailsStr() {
            //    try { return JsonConvert.SerializeObject(details); }
            //    catch { return str; }
            //}
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
