using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace StandardApiTools
{

    /// <summary>
    /// Filtro de execução de contexto que converte criar um <see cref="StdApiErrorResult"/>
    /// para o contexto caso ele ainda tenha uma exceção não tratada.
    /// </summary>
    /// <remarks>
    /// Pode-se usar esta classe de 2 formas:
    /// <br/>1. Configurando-a como um filtro na estrutura do MVC;
    /// <br/>2. Chamando o método estático <see cref="Handle"/> em um filtro já configurado.
    /// </remarks>
    /// indicadas por <see cref="ActionExecutedContext.Exception"/> e <see cref="ActionExecutedContext.ExceptionHandled"/>
    public class StdApiContextFilter : ActionFilterAttribute
    {

        /// <summary>
        /// Define o <see cref="ActionExecutedContext.Result"/> como um <see cref="EasyResponseResult(Exception)"/> caso
        /// exista uma exceção não tratada no contexto.
        /// </summary>
        public static void Handle(ActionExecutedContext context)
        {
            if (context.Exception != null && !context.ExceptionHandled)
            {
                var result = GetResult(context.Exception);
                if (result == null) return;
                context.Result = result;
                context.ExceptionHandled = true;
            }
        }




        public static void Handle(ExceptionContext context)
        {
            if (context.Exception != null && !context.ExceptionHandled)
            {
                var result = GetResult(context.Exception);
                if (result == null) return;
                context.Result = result;
                context.ExceptionHandled = true;
            }
        }




        public static StdApiResult GetResult(Exception ex) {
            ex = ex.Deaggregate();
            var result = ex is IProduceStdApiErrorResult pr
            ? pr.ToResult()
            : new StdApiResult(HttpStatusCode.InternalServerError, ex.ToString());
            return result;
        }




        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Handle(context);
            base.OnActionExecuted(context);
        }
    }
}
