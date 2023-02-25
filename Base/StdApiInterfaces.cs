namespace StandardApiTools {

    public interface IProduceStdApiErrorResult : IProduceStdApiResult {
        new StdApiErrorResult ToResult();
    }


    public interface IProduceStdApiResult {
        StdApiResult ToResult();
    }


    public interface IAddInfo {
        public void AddInfo(string key, object value);
    }
}
