namespace StandardApiTools {

    public interface IProduceStdApiErrorResult {
        public StdApiErrorResult ToResult();
    }

    public interface IAddInfo {
        public void AddInfo(string key, object value);
    }
}
