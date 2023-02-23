using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace StandardApiTools {

    public interface IProduceStdApiResult {
        public StdApiResult GetResult();
    }




    public class StdApiResult<T>: StdApiResult where T : class {

        public StdApiResult(int status, string message, T data) : base(status, message, data) { }

        public StdApiResult(StdApiResponse response, string message) {
            base.Status = response.StatusCode;
            base.Content = Desserialize(response.ContentAsString);
            base.Message = ComposeMessage(message, Content);
        }

        public new T Content => (T)base.Content;

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




    public class StdApiResult: IActionResult {

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
            CustomData = new Dictionary<string, object>();
            SupressNullFromResult = SupressNullPropertiesFromResults;
        }
        public StdApiResult(StdApiResponse response, string message)
        : this(response.StatusCode, message, response.ContentAsString) { }
        public StdApiResult(StdApiResponse response)
        : this(response, response.CommMessage) { }
        protected StdApiResult() { }




        public int Status { get; protected set; }
        public string Message { get; protected set; }
        public object Content { get; protected set; }
        public object Info { get; protected set; }
        public bool SupressNullFromResult { get; set; }
        public string CustomDataGroupName { get; set; }
        public static bool SupressNullPropertiesFromResults { get; set; } = true;
        public Dictionary<string, object> CustomData { get; protected set; }




        public async Task ExecuteResultAsync_old(ActionContext context) {
            var r = new JsonResult(null) {
                StatusCode = Status,
                ContentType = "application/json",
            };
            if (!SupressNullFromResult)
                r.Value = new { message = Message, content = Content, info = Info };
            else if (Content == null && Info == null)
                r.Value = new { message = Message };
            else if (Info == null)
                r.Value = new { message = Message, content = Content };
            else if (Content == null)
                r.Value = new { message = Message, info = Info };
            else
                r.Value = new { message = Message, content = Content, info = Info };
            await r.ExecuteResultAsync(context);
        }




        public async Task ExecuteResultAsync(ActionContext context) {
            var r = new JsonResult(null) {
                StatusCode = Status,
                ContentType = "application/json",
            };
            r.Value = CompileFinalDictionary();
            await r.ExecuteResultAsync(context);
        }




        public Dictionary<string, object> CompileFinalDictionary() {
            var dict = new Dictionary<string, object>();
            //var opt = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true };
            if (!SupressNullFromResult || Message != null) dict.Add("message", Message);
            if (!SupressNullFromResult || Content != null) dict.Add("content", Message);
            var group = CustomDataGroupName == null ? dict : new Dictionary<string, object>();
            if (CustomDataGroupName == null) {
                foreach (var entry in CustomData) {
                    int keyCount = 2;
                    var key = entry.Key;
                    while (group.ContainsKey(key)) key = $"{entry.Key}({keyCount++})";
                    if (!SupressNullFromResult || entry.Value != null) group.Add(key, entry.Value);
                }
            }
            if (!ReferenceEquals(group, dict)) dict.Add(CustomDataGroupName ?? "Data", group);
            return dict;
        }




        private bool HasCountSuffix(string str) {
            var span = str.AsSpan();
            return span.Slice(-3, 1)[0] == '(' && span.Slice(-1, 1)[0] == ')' && char.IsDigit(span.Slice(-2, 1)[0]);
        }

        private int GetCountSuffix(ReadOnlySpan<char> span) {
            return span.Slice(-2, 1)[0].ToDigit() ?? 0;
        }
    }
}
