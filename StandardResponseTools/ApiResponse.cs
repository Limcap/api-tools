using System.Net;
using System;
using Microsoft.AspNetCore.Mvc;

namespace StandardResponseTools {
    public class ApiResponse: JsonResult {
        public ApiResponse(int status, string message, object data = null) : base(null) {
            StatusCode = status;
            Value = new { message, data };
            //Value = string.IsNullOrEmpty(message) ? data : new { message, data };// new DadosDeResposta { Message = message, Data = data };
        }



        public ApiResponse(Exception ex) : base(null) {
            if (ex is WebException wex) ex = new ExternalServiceException(wex);
            if (ex is ISRReady aex) set(aex.Status, aex.Message, aex.Details);
            else set(500, ex.Message, ex.StackTrace);
            void set(int status, string message, object data) {
                StatusCode = status;
                Value = new { message, data };
                //Value = string.IsNullOrEmpty(message) ? data : new { message, data }; //new ResponseData { Message = message, Data = data };
            }
        }



        public struct Content {

            /// <summary>
            /// Mensagem descritiva do resultado.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Dados do resultado.
            /// </summary>
            public object Data { get; set; }
        }
    }
}
