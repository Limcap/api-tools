using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.ApiTools.Mvc {
	internal class ActionResult : IActionResult {

		public int StatusCode;
		public object CompiledResultObject;

		public virtual async Task ExecuteResultAsync( ActionContext context ) {

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
			try {
				var str = JsonConvert.SerializeObject(CompiledResultObject);
				var resp = context.HttpContext.Response;
				var bytes = Encoding.UTF8.GetBytes(str);
				resp.ContentType = "application/json; charset=utf-8";
				resp.ContentLength = bytes.Length;
				resp.StatusCode = StatusCode;
				using (var c = context.HttpContext.Response.Body) await c.WriteAsync(bytes, 0, bytes.Length);
			}
			catch (Exception ex) {
				throw new Exception("Error when executing AspNetCore.MVC result", ex);
			}
		}
	}
}
