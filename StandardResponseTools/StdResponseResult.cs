using System.Net;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace StdResponseTools {

    public interface IStdResponseResult {
        public StdResponseResult Result { get; }
    }






    public class StdResponseResult<T> : StdResponseResult {
        public StdResponseResult(int status, string message, T data) : base(status, message, data) { }
        public new T Data => (T) base.Data;
    }





    public class StdResponseResult : IActionResult {

        public StdResponseResult(int status, string message, object data = null) { 
            Status = status;
            Message = message;
            Data = data;
        }






        public static StdResponseResult From(Exception ex) {
            if (ex is StdResponseException erex) {
                return erex.Result;
            }
            if (ex is IStdResponseResult aex) {
                return aex.Result;
            }
            return new StdResponseResult(
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
