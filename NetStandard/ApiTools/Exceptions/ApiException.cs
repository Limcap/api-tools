using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;

namespace Limcap.ApiTools {

	public class ApiException : ApiExceptionBase, IAddInfo {

		protected ApiException( WebException innerException, string message = null )
		: base(message, innerException) {
			this.info = new DataCollection(new Dictionary<string, object>(3));
		}




		protected ApiException( Exception sourceException, string message = null )
		: base(message ?? Status500DefaultMessage, sourceException) {
			this.statusCode = 500;
			this.details = sourceException.ToString();
			this.info = new DataCollection(new Dictionary<string, object>(3));
		}




		public ApiException( string message, object details = null )
		: this(500, message, details) { }




		public ApiException( HttpStatusCode code, string message = null, object details = null )
		: this((int)code, message ?? code.ToString(), details) { }




		public ApiException( CommunicationStatus code, string message = null, object details = null )
		: this((int)code, message ?? code.ToString(), details) { }




		public ApiException( int code, string message = null, object details = null )
		: base(message) {
			this.statusCode = code;
			this.details = details;
			this.info = new DataCollection(new Dictionary<string, object>(3));
		}




		public new DataCollection Info { get => info as DataCollection; }




		IAddInfo IAddInfo.AddInfo( string key, object value ) => this.AddInfo(key, value);




		public virtual ApiException SetStatus( HttpStatusCode status ) {
			statusCode = (int)status;
			return this;
		}




		public virtual ApiException SetDetails( object details ) {
			this.details = details;
			return this;
		}



		public ApiErrorResult ToResult( bool includeStackTraceInfo ) {
			if (includeStackTraceInfo) Info.Add("StackTrace", StackTrace);
			return base.ToResult();
		}




		public virtual ApiException SourceException() {
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
			var ex = new ApiException(s, m, d);
			if (j.TryGetValue("info", out _) && j["info"].Type == JTokenType.Array) {
				var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(j["info"].ToString());
				ex.info = new DataCollection(dic);
			}
			return ex.SourceException();
			//string detailsStr() {
			//    try { return JsonConvert.SerializeObject(details); }
			//    catch { return str; }
			//}
		}




		public static ApiException From( Exception ex, string message = null ) {
			ex = ex.Deaggregate();
			if (ex is ApiException ex2) {
				if (message != null && !ex2.MessageParts.Contains(message)) ex2.InsertMessage(message);
				return ex2;
			}
			else return new ApiException(ex, message);
		}




		private static string Status500DefaultMessage = "Ocorreu um erro não identificado durante o processamento.";
	}
}
