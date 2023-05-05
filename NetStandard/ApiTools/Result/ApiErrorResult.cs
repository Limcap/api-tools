using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading.Tasks;

namespace Limcap.ApiTools {
	public partial class ApiErrorResult : ApiResult {

		public ApiErrorResult( int status, string message, object details = null,
		object info = null, bool? suppressNullValues = null ) {
			StatusCode = status;
			Message = message;
			Details = details;
			Info = info;
			SupressNullValues = suppressNullValues ?? SupressNullValuesFromResults;
		}




		public ApiErrorResult( StdResponse response, string message )
		: this(response.StatusCode, message, response.ContentAsString) { }




		public ApiErrorResult( StdResponse response )
		: this(response, response.CommMessage) { }




		public string Message { get; private set; }
		public object Details { get; private set; }
		public object Info { get; private set; }
		public override object CompiledResultObject => Compile();



		public bool SupressNullValues;
		public string CustomDataGroupName;




		public DataCollection GetInfoDataCollection() {
			Info = Info ?? new DataCollection();
			return Info as DataCollection;
		}




		public object Compile() {
			var eo = new ExpandoObject();
			var eoc = (ICollection<KeyValuePair<string, object>>)eo;
			if (!SupressNullValues || Message != null)
				eoc.Add(new KeyValuePair<string, object>(MessageKeyName, Message));
			if (!SupressNullValues || Details != null)
				eoc.Add(new KeyValuePair<string, object>(DetailsKeyName, Details));
			var info = (Info as DataCollection)?.ToObject(SupressNullValues) ?? Info;
			if (!SupressNullValues || Info != null)
				eoc.Add(new KeyValuePair<string, object>(InfoKeyName, info));
			return eo;
		}




		public static bool SupressNullValuesFromResults = false;
		public static string MessageKeyName = "message";
		public static string DetailsKeyName = "details";
		public static string InfoKeyName = "info";




		public static ApiErrorResult CreateFrom( Exception ex, string message = null ) {
			return ApiException.From(ex, message).ToResult();
			//if (ex == null) return null;
			//ex = ex.Deaggregate();
			//var result = ex is IProduceStdApiErrorResult pr
			//? pr.ToResult()
			//: StdApiException.CreateFrom(ex, "Ocorreu um erro não identificado durante o processamento.").ToResult();
			//return result;
		}
	}
}
