namespace Limcap.ApiTools {

	public interface IMakeApiErrorResult : IMakeApiResult {
		new ApiErrorResult ToResult();
	}


	public interface IMakeApiResult {
		ApiResult ToResult();
	}


	public interface IAddInfo {
		IAddInfo AddInfo( string key, object value );
	}
}
