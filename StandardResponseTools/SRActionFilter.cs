using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace StandardResponseTools {

    /// <summary>
    /// Filtro de execução de contexto que converte criar um <see cref="SRResult"/>
    /// para o contexto caso ele ainda tenha uma exceção não tratada.
    /// </summary>
    /// <remarks>
    /// Pode-se usar esta classe de 2 formas:
    /// <br/>1. Configurando-a como um filtro na estrutura do MVC;
    /// <br/>2. Chamando o método estático <see cref="HandleExceptions"/> em um filtro já configurado.
    /// </remarks>
    /// indicadas por <see cref="ActionExecutedContext.Exception"/> e <see cref="ActionExecutedContext.ExceptionHandled"/>
    public class SRActionFilter: ActionFilterAttribute {

        /// <summary>
        /// Define o <see cref="ActionExecutedContext.Result"/> como um <see cref="SRResult(Exception)"/> caso
        /// exista uma exceção não tratada no contexto.
        /// </summary>
        public static void HandleExceptions(ActionExecutedContext context) {
            if (context.Exception != null && !context.ExceptionHandled) {
                context.Exception = context.Exception is AggregateException ex1 && ex1.InnerExceptions.Count == 1 ? ex1.InnerExceptions[0] : context.Exception;
                context.Result = new SRResult(context.Exception);
                context.ExceptionHandled = true;
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context) {
            HandleExceptions(context);
            base.OnActionExecuted(context);
        }
    }
}
