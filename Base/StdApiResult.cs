using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.SymbolStore;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StandardApiTools {

    public partial class StdApiResult: IActionResult {

        public static StdApiResult From(Exception ex) {
            if (ex is IProduceStdApiResult aex) {
                return aex.ToResult();
            }
            return new StdApiResult(
                500,
                "Ocorreu um erro não identificado durante o processamento.",
                new { ErrorMessage = ex.Message, Detail = ex.ToString() }
            );
        }




        public StdApiResult(int status, string message, object content = null, StdApiDataCollection info = null) {
            Status = status;
            Message = message;
            Content = content;
            CustomInfo = info ?? new StdApiDataCollection();
            SupressNullFromResult = SupressNullPropertiesFromResults;
        }




        public StdApiResult(StdApiResponse response, string message)
        : this(response.StatusCode, message, response.ContentAsString) { }




        public StdApiResult(StdApiResponse response)
        : this(response, response.CommMessage) { }




        protected StdApiResult() { }




        //public int GetStatus() => Status;
        public readonly int Status; //{ get; protected set; }
        public string Message { get; protected set; }
        public object Content { get; protected set; }
        public object Info { get => CustomInfo.ToObject(); }
        public readonly IStdApiDataCollection CustomInfo;




        public static bool SupressNullPropertiesFromResults = false;
        public bool SupressNullFromResult;
        public string CustomDataGroupName;




        public StdApiResult AddInfo(string key, object value) {
            CustomInfo.Add(key, value);
            return this;
        }




        public object CompileFullObject() {
            var dict = new Dictionary<string, object>();
            dict.Add("message", Message);
            if (!SupressNullFromResult || Content != null) dict.Add("content", Content);
            if (!SupressNullFromResult || CustomInfo.Count > 0) dict.Add("data", CustomInfo.ToObject());
            var opt = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true };
            var serialized = JsonSerializer.Serialize(dict, opt);
            var obj = JsonSerializer.Deserialize<object>(serialized, opt);
            return obj;
        }




        public async Task ExecuteResultAsync(ActionContext context) {
            var eo = new ExpandoObject();
            var eoc = (ICollection<KeyValuePair<string, object>>)eo;
            if (!SupressNullFromResult || Message != null)
                eoc.Add(new KeyValuePair<string, object>("message", Message));
            if (!SupressNullFromResult || Content != null)
                eoc.Add(new KeyValuePair<string, object>("content", Content));
            if (!SupressNullFromResult || CustomInfo.Count > 0)
                eoc.Add(new KeyValuePair<string, object>("data", CustomInfo.ToObject(SupressNullFromResult)));
            var r = new JsonResult(eoc) {
                ContentType = "application/json",
                StatusCode = Status
            };
            await r.ExecuteResultAsync(context);
        }
    }
}
