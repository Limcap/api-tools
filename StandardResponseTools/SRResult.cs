using System.Net;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace StandardResponseTools {

    public class SRResult<T> : SRResult {
        public SRResult(int status, string message, T data) : base(status, message, data) { }
        public new T Data => (T) base.Data;
    }





    [DataContract]
    public class SRResult : IActionResult {

        public SRResult(int status, string message, object data = null) { 
            Status = status;
            Message = message;
            Data = data;
        }






        public SRResult(Exception ex) {
            if (ex is WebException wex)
                ex = new ExternalServiceException(wex);
            if (ex is ISRReady aex) { Status = aex.Status; Message = aex.Message; Data = aex.Details; }
            else { Status = 500; Message = "Ocorreu um erro não identificado durante o processamento.";
                Data = ex.ToString(); }
        }





        public readonly int Status;
        public string Message { get; private set; }
        public object Data { get; private set; }






        public async Task ExecuteResultAsync(ActionContext context) {
            await new JsonResult(null) {
                StatusCode = Status,
                ContentType = "application/json",
                Value = new { message = Message, data = Data }
            }.ExecuteResultAsync(context);
        }
    }
}
