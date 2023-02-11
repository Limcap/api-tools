using System.Net;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace StandardApiTools {

    public interface IProduceStdApiResult {
        public StdApiResult Result { get; }
    }




    public class StdApiResult<T> : StdApiResult {
        public StdApiResult(int status, string message, T data) : base(status, message, data) { }
        public new T Data => (T) base.Data;
    }






    public class StdApiResult : IActionResult {

        public StdApiResult(int status, string message, object data = null) { 
            Status = status;
            Message = message;
            Data = data;
        }




        public static StdApiResult From(Exception ex) {
            if (ex is StdApiWebException erex) {
                return erex.Result;
            }
            if (ex is IProduceStdApiResult aex) {
                return aex.Result;
            }
            return new StdApiResult(
                500,
                "Ocorreu um erro não identificado durante o processamento.",
                new { Message = ex.Message, Data = ex.ToString() }
            );
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
