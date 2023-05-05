using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Limcap.ApiTools {
	public partial class ApiWebException : ApiException {

		/// <summary>
		/// Construtor privado pois é preciso validar primeiro a response pois
		/// só deve ser criado uma exceção se a resposta for de erro.
		/// <seealso cref="From(StdResponse, string)"/>
		/// </summary>
		public ApiWebException( StdResponse response, bool unwrapped = false )
		: base(response.Exception) {
			Response = response;
			statusCode = 424;
			this.AddMessage(_defaultMsg);
			DetailsDeserializer = DefaultDetailsDeserializer;
			if (unwrapped) {
				isUnwrapped = true;
				this.SetMessage(Response.CommMessage);
			}
		}




		public readonly StdResponse Response;




		public override int StatusCode => isUnwrapped ? Response.StatusCode : statusCode;





		public new ApiWebException SetStatus( HttpStatusCode status ) {
			statusCode = (int)status;
			return this;
		}




		public override object Details
		=> details != null ? details : CompileDetails();





		public new ApiWebException SetDetails( object details ) {
			this.details = details;
			return this;
		}




		#region ============================================================================
		#endregion
		// Relativo a conversão para resultado

		private object CompileDetails() {
			if (isUnwrapped) return DetailsDeserializer(Response.ContentAsString);
			else return new StdApiWebExceptionDetails {
				Status = Response.HttpStatus != null
							? (int)Response.HttpStatus
							: (int)Response.CommStatus,
				Message = Response.CommMessage,
				Details = DetailsDeserializer(Response.ContentAsString),
				Uri = Response.RequestUri
			};
		}




		private Func<string, object> DetailsDeserializer;
		private Func<string, object> DefaultDetailsDeserializer
				=> s => s.TryDeserialize(out var r) == null ? r : null;




		public ApiWebException SetResponseContentType<T>() {
			var infoTitle = "Erro de desserialização";
			var infoText = "O conteúdo está apresentado no formato string, pois não foi possível desserializá-lo.";
			DetailsDeserializer = s => {
				if (s == null || s[0] != '{' && s[0] != '[') return s;
				try {
					var result = JsonUtil.Deserialize<T>(s);
					Info.Del(infoTitle);
					return result;
				}
				catch (Exception e) {
					Info.Set(infoTitle, infoText + Environment.NewLine + e.Message);
					return s;
				}
			};
			return this;
		}




		private bool isUnwrapped;




		public ApiWebException UnwrapStatus( int status ) {
			if (Response.StatusCode != status) return null;
			isUnwrapped = true;
			this.SetMessage(Response.CommMessage);
			return this;
		}
		public ApiWebException UnwrapStatus() {
			isUnwrapped = true;
			this.SetMessage(Response.CommMessage);
			return this;
		}




		

		public override ApiException SourceException() {
			if (StatusCode != 424) return this;
			if (Response.CommStatus != CommunicationStatus.Success && string.IsNullOrEmpty(Response.ContentAsString)) {
				return new ApiException(Response.CommStatus, Response.CommMessage, Response.ContentAsString);
			}
			var j = JObject.Parse(Response.ContentAsString);
			if (!j.TryGetValue("message", out _) || !j.TryGetValue("details", out _))
				return this;
			var m = j["message"].ToString();
			var d = j["details"].ToString();
			var ex = new ApiException(Response.StatusCode, m, d);
			if (j.TryGetValue("info", out _) && j["info"].Type == JTokenType.Array) {
				var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(j["info"].ToString());
				foreach (var item in dic) ex.AddInfo(item.Key, item.Value);
			}
			return ex.SourceException();
		}






		//private class InnerData {
		//    public int StatusCode { get; set; }
		//    public string Message { get; set; }
		//    public object Details { get; set; }
		//    public Dictionary<string, object> Info { get; set; }
		//    public InnerData(StdApiResponse resp) {
		//        StatusCode = resp.StatusCode;
		//        Message = resp.CommMessage;
		//        Details = resp.ContentAsString;
		//    }
		//    public StdApiException ToException() {
		//        var ex = new StdApiException(
		//            (System.Net.HttpStatusCode)StatusCode,
		//            Message,
		//            Details
		//        );
		//        if (Info != null && Info.Count > 0)
		//            foreach (var item in Info)
		//                ex.Info.Add(item.Key, item.Value);
		//        return ex;
		//    }
		//}




		#region ============================================================================
		#endregion

		public static R Handle<R>( Func<R> function, Action<ApiWebException> exceptionCusomization ) {
			try {
				return function();
			}
			catch (ApiWebException ex) {
				exceptionCusomization(ex);
				throw;
			}
		}



		public static void Handle<E>( Action function, Action<ApiWebException> exceptionCusomization ) {
			try {
				function();
			}
			catch (ApiWebException ex) {
				exceptionCusomization(ex);
				throw;
			}
		}




		public static async Task<R> HandleAsync<R>( Func<Task<R>> function, Action<ApiWebException> exceptionCusomization ) {
			try {
				return await function();
			}
			catch (ApiWebException ex) {
				exceptionCusomization(ex);
				throw;
			}
		}




		public static async Task HandleAsync<E>( Func<Task> function, Action<ApiWebException> exceptionCusomization ) {
			try {
				await function();
			}
			catch (ApiWebException ex) {
				exceptionCusomization(ex);
				throw;
			}
		}




		#region ============================================================================
		#endregion
		// Static e Constants

		/// <summary>
		/// Wrapper para o contrutor privado com os mesmos parâmetros, para validar a construção,
		/// pois não é permitido criar um StdApiWebException a partir
		/// de uma <see cref="StdResponse"/> cujo <see cref="StdResponse.CommStatus"/> seja
		/// <see cref="StdResponse.CommunicationStatus.Success"/>
		/// </summary>
		public static ApiWebException From( StdResponse response ) {
			if (response.IsSuccess) return null;
			var ex = new ApiWebException(response);
			return ex;
		}




		private const string _defaultMsg = "A chamada para um serviço externo falhou.";




		public static ApiException CreateManually( object details ) {
			return new ApiException(424, _defaultMsg, details);
		}
	}








	public class StdApiWebExceptionDetails : StdApiWebExceptionDetails<object> { }
	public class StdApiWebExceptionDetails<T> {
		public int Status { get; set; }
		public string Message { get; set; }
		public T Details { get; set; }
		public Uri Uri { get; set; }
	}
}
