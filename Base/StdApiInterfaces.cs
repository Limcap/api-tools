namespace StandardApiTools {

    public interface IProduceStdApiResult {
        public StdApiErrorResult ToResult();
    }

    public interface IAddInfo {
        public void AddInfo(string key, object value);
    }
}
