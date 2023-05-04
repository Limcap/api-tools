using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text.Json;
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

            // Ao usar JsonResult, não é possível definir o charset para a resposta, e
            // os caracteres acentuados acabam ficando alterados. Também não é possível
            // definir o Newtonsoft.Json.JsonSerializerSettings presente como parâmetro
            // em um dos construtores do JsonResult pois ocorre um erro falando que esse
            // construtor não foi encontrado, provavelmente pq a versão não corresponde
            // então nesse caso, melhor não usar pois não sabemos qual a versão o chamdor
            // estará usando.
            //var r = new JsonResult(str) {
            //    ContentType = "application/json",
            //    StatusCode = StatusCode,
            //    //SerializerSettings = new JsonSerializerSettings { Culture = System.Globalization.CultureInfo.CurrentCulture };
            //};
            //await r.ExecuteResultAsync(context);

            // Usar o ContentResult é melhor pois temos mais controle, mas persiste o problema
            // de não poder especificar o charset
            //var str = JsonConvert.SerializeObject(CompiledResultObject);
            //var r = new Microsoft.AspNetCore.Mvc.ContentResult() {
            //    Content = str,
            //    ContentType = "application/json",
            //    StatusCode = StatusCode
            //};
            //// -- ou assim
            //var b = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ContentResult>>();
            //await b.ExecuteAsync(context, r);
            //// -- ou assim
            //await r.ExecuteResultAsync(context);

            // O melhor jeito é definir a resposta manualmente
            var str = JsonConvert.SerializeObject(CompiledResultObject);
            var resp = context.HttpContext.Response;
            var bytes = Encoding.UTF8.GetBytes(str);
            resp.ContentType = "application/json; charset=utf-8";
            resp.ContentLength = bytes.Length;
            resp.StatusCode = StatusCode;
            using (var c = context.HttpContext.Response.Body) await c.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
