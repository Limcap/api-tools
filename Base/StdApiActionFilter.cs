using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace StandardApiTools {

    /// <summary>
    /// Filtro de execução de contexto que converte criar um <see cref="StdApiErrorResult"/>
    /// para o contexto caso ele ainda tenha uma exceção não tratada.
    /// </summary>
    /// <remarks>
    /// Pode-se usar esta classe de 2 formas:
    /// <br/>1. Configurando-a como um filtro na estrutura do MVC;
    /// <br/>2. Chamando o método estático <see cref="HandleExceptions"/> em um filtro já configurado.
    /// </remarks>
    /// indicadas por <see cref="ActionExecutedContext.Exception"/> e <see cref="ActionExecutedContext.ExceptionHandled"/>
    public class StdApiActionFilter: ActionFilterAttribute {

        /// <summary>
        /// Define o <see cref="ActionExecutedContext.Result"/> como um <see cref="EasyResponseResult(Exception)"/> caso
        /// exista uma exceção não tratada no contexto.
        /// </summary>
        public static void HandleException(ActionExecutedContext context) {
            if (context.Exception != null && !context.ExceptionHandled) {
                var result = GetResultFromException(context.Exception);
                if (result == null) return;
                context.Result = result;
                context.ExceptionHandled = true;
            }
        }




        public static void HandleExceptions(ExceptionContext context) {
            if (context.Exception != null && !context.ExceptionHandled) {
                var result = GetResultFromException(context.Exception);
                if (result == null) return;
                context.Result = result;
                context.ExceptionHandled = true;
            }
        }




        public static StdApiErrorResult GetResultFromException(Exception ex) {
            if (ex == null) return null;
            ex = ex.Deaggregate();
            var result = ex is IProduceStdApiErrorResult pr
            ? pr.ToResult()
            : new StdApiException(ex, "Ocorreu um erro não identificado durante o processamento.").ToResult();
            return result;
        }




        public override void OnActionExecuted(ActionExecutedContext context) {
            HandleException(context);
            base.OnActionExecuted(context);
        }
    }
}
