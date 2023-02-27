using Microsoft.AspNetCore.Mvc.Filters;

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
        public static void HandleExceptions(ActionExecutedContext context) {
            if (context.Exception != null && !context.ExceptionHandled) {
                context.Exception = context.Exception.Deaggregate();
                //context.Result = StdApiErrorResult.CreateFrom(context.Exception);
                context.Result = context.Exception is IProduceStdApiResult pr
                ? pr.ToResult()
                : new StdApiException(context.Exception, "Ocorreu um erro não identificado durante o processamento.").ToResult();
                context.ExceptionHandled = true;
            }
        }
        public static void HandleExceptions(ExceptionContext context) {
            if (context.Exception != null && !context.ExceptionHandled) {
                context.Exception = context.Exception.Deaggregate();
                //context.Result = StdApiErrorResult.CreateFrom(context.Exception);
                context.Result = context.Exception is IProduceStdApiResult pr
                ? pr.ToResult()
                : new StdApiException(context.Exception, "Ocorreu um erro não identificado durante o processamento.").ToResult();
                context.ExceptionHandled = true;
            }
        }




        public override void OnActionExecuted(ActionExecutedContext context) {
            HandleExceptions(context);
            base.OnActionExecuted(context);
        }
    }
}
