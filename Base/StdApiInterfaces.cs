namespace StandardApiTools {

    public interface IProduceStdApiErrorResult : IProduceStdApiResult {
        new StdApiErrorResult ToResult();
    }


    public interface IProduceStdApiResult {
        StdApiResult ToResult();
    }


    public interface IAddInfo {
        public IAddInfo AddInfo(string key, object value);
    }



    public interface IErrorToResultConverter<T> : IProduceStdApiErrorResult where T : StdApiExceptionBase {
        void SetSourceException(T excpetion);
    }



    //public abstract class StdApiCustomErrorResult<T>: IProduceStdApiErrorResult where T : StdApiExceptionBase {
    //    protected T exception;
    //    public void SetSourceException(T exception) {
    //        this.exception = exception;
    //    }
    //    public abstract StdApiErrorResult ToResult();
    //    StdApiResult IProduceStdApiResult.ToResult() => ToResult();
    //}



    public abstract class StdApiCustomErrorResult<T>: IProduceStdApiErrorResult where T : StdApiExceptionBase {
        protected StdApiCustomErrorResult(T exception) => this.exception = exception;
        private T exception;
        protected abstract object CompileResultContent(object exceptionContent);
        public StdApiErrorResult ToResult() => new StdApiErrorResult(exception.StatusCode, exception.Message, CompileResultContent(exception.Content), exception.Info, true);
        StdApiResult IProduceStdApiResult.ToResult() => ToResult();
    }




    //public class ObterConteudoVinculadoErrorResult404: IErrorToResultConverter<StdApiWebException> {
    //    public void Receive(StdApiWebException exception) {
    //        throw new System.NotImplementedException();
    //    }

    //    public StdApiResult ToResult() {
    //        throw new System.NotImplementedException();
    //    }
    //}
}
