using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Limcap.ApiTools.AspNetStandard {
	public class ApiExceptionHandlerAttribute : ExceptionFilterAttribute {
		public override void OnException( HttpActionExecutedContext context ) {
			context.Response = ApiException.From(context.Exception).ToResult().ToHttpResponseMessage();
		}
	}
}
