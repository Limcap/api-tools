using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using StandardApiTools;

namespace StandardApitTools {
    public class StdApiExceptionHandlerAttribute : ExceptionFilterAttribute {
        public override void OnException(HttpActionExecutedContext context) {
            if (context.Exception is StdApiException ex) {
                context.Response = ex.ToResult();
            }
        }
    }
}
