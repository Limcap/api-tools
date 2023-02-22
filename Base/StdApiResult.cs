using System.Net;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Reflection.Metadata;

namespace StandardApiTools {

    public interface IProduceStdApiResult {
        public StdApiResult GetResult();
    }




    public class StdApiResult<T> : StdApiResult where T : class {
 
        public StdApiResult(int status, string message, T data) : base(status, message, data) { }
 
        public StdApiResult(StdApiResponse response, string message) {
            base.Status = response.StatusCode;
            base.Content = Desserialize(response.ContentAsString);
            base.Message = ComposeMessage(message, Content);
        }

        public new T Content => (T) base.Content;

        public static T Desserialize(string json, JsonSerializerOptions options = null) {
            options ??= new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            try { return JsonSerializer.Deserialize<T>(json, options); }
            catch { return null; }
        }

        public static string ComposeMessage(string message, T content) {
            if (content != null) return message;
            return message
            + Environment.NewLine
            + "\nO conteúdo não pode ser desserializado em um objeto do tipo " + typeof(T).Name;
        }
    }




    public class StdApiResult : IActionResult {

        public static StdApiResult From(Exception ex) {
            if (ex is IProduceStdApiResult aex) {
                return aex.GetResult();
            }
            return new StdApiResult(
                500,
                "Ocorreu um erro não identificado durante o processamento.",
                new { ErrorMessage = ex.Message, Detail = ex.ToString() }
            );
        }




        public StdApiResult(int status, string message, object content = null, object info = null) {
            Status = status;
            Message = message;
            Content = content;
            Info = info;
        }
        public StdApiResult(StdApiResponse response, string message)
        : this (response.StatusCode, message, response.ContentAsString) {}
        public StdApiResult(StdApiResponse response)
        : this(response, response.CommMessage) {}
        protected StdApiResult(){}




        public int Status { get; protected set; }
        public string Message { get; protected set; }
        public object Content { get; protected set; }
        public object Info { get; protected set; }
        public bool SupressNullFromResult { get; set; } = SupressNullPropertiesFromResults;
        public static bool SupressNullPropertiesFromResults { get; set; } = true;




        public async Task ExecuteResultAsync(ActionContext context) {
            var r = new JsonResult(null) {
                StatusCode = Status,
                ContentType = "application/json",
            };
            if (!SupressNullFromResult)
                r.Value = new { message = Message, content = Content, info = Info };
            else if (Content == null && Info == null)
                r.Value = new { message = Message };
            else if (Info == null)
                r.Value = new { message = Message, content = Content};
            else if (Content == null)
                r.Value = new { message = Message, info = Info};
            await r.ExecuteResultAsync(context);
        }
    }
}
