using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace StandardApiTools {

    public class StdApiResult: IActionResult {

        protected StdApiResult() { }

        public StdApiResult(HttpStatusCode status, object content) {
            StatusCode = (int)status;
            CompiledResultObject = content;
        }

        public int StatusCode;
        public object CompiledResultObject;

        public virtual async Task ExecuteResultAsync(ActionContext context) {
            var r = new JsonResult(CompiledResultObject) {
                ContentType = "application/json",
                StatusCode = StatusCode
            };
            await r.ExecuteResultAsync(context);
        }
    }
}
