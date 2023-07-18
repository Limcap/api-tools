using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace Limcap.ApiTools.AspNetLegacy.HttpFilters {
	public class ApiExceptionHandlerAttribute : System.Web.Http.Filters.ExceptionFilterAttribute {
		public override void OnException( HttpActionExecutedContext context ) {
			context.Response = ApiException.From(context.Exception).ToResult().ToHttpResponseMessage();
		}
	}
}


namespace Limcap.ApiTools.AspNetLegacy.MvcFilters {
	public class ApiExceptionHandlerAttribute : System.Web.Mvc.ActionFilterAttribute {
		public override void OnActionExecuted( ActionExecutedContext context ) {
			if(context.Exception == null || context.ExceptionHandled ) {
				base.OnActionExecuted(context);
			}
			else {
				var result = ApiException.From(context.Exception).ToResult();
				var content = JsonConvert.SerializeObject(result.GetCompiledResultObject());
				var r = context.HttpContext.Response;
				r.StatusCode = result.GetStatusCode();
				r.ContentType = "application/json";
				r.Write(content);
				context.ExceptionHandled = true;
			}
		}
	}

}
