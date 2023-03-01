using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace StandardApiTools {
    public partial class StdApiWebException: StdApiException {

        /// <summary>
        /// Construtor privado pois é preciso validar primeiro a response pois
        /// só deve ser criado uma exceção se a resposta for de erro.
        /// <seealso cref="From(StdApiResponse, string)"/>
        /// </summary>
        public StdApiWebException(StdApiResponse response)
        : base(response.Exception) {
            Response = response;
            this.AddMessage(_defaultMsg);
            DetailsDeserializer = DefaultDetailsDeserializer;
        }




        public readonly StdApiResponse Response;




        public override int StatusCode
        => statusCode > 0 ? statusCode : isUnwrapped ? Response.StatusCode : (int)HttpStatusCode.FailedDependency;





        public new StdApiWebException SetStatus(HttpStatusCode status) {
            statusCode = (int)status;
            return this;
        }




        public override object Details
        => details != null ? details : CompileDetails();





        public new StdApiWebException SetDetails(object details) {
            this.details = details;
            return this;
        }




        #region ============================================================================
        #endregion
        // Relativo a conversão para resultado

        private object CompileDetails() {
            if (isUnwrapped) return DetailsDeserializer(Response.ContentAsString);
            else return new StdApiWebExceptionDetails {
                Status = Response.HttpStatus != null
                    ? (int)Response.HttpStatus
                    : (int)Response.CommStatus,
                Message = Response.CommMessage,
                Details = DetailsDeserializer(Response.ContentAsString),
                Uri = Response.RequestUri
            };
        }




        private Func<string, object> DetailsDeserializer;
        private Func<string, object> DefaultDetailsDeserializer => s => s.TryDeserialize(out var r) ? r : null;




        public StdApiWebException SetResponseContentType<T>(JsonSerializerOptions opt = null) {
            var s = Response?.ContentAsString.TrimToNull();
            var infoTitle = "Erro de desserialização";
            var infoText = "O conteúdo está apresentado no formato string, pois não foi possível desserializá-lo.";
            DetailsDeserializer = s => {
                if (s == null || s[0] != '{' && s[0] != '[') return s;
                try {
                    var result = JsonUtil.Deserialize<T>(s, opt);
                    Info.Del(infoTitle);
                    return result;
                }
                catch (Exception e) {
                    Info.Set(infoTitle, infoText + Environment.NewLine + e.Message);
                    return s;
                }
            };
            return this;
        }




        private bool isUnwrapped;




        public StdApiWebException UnwrapStatus(int status) {
            if (Response.StatusCode != status) return null;
            isUnwrapped = true;
            this.SetMessage(Response.CommMessage);
            return this;
        }
        public StdApiWebException UnwrapStatus() {
            isUnwrapped = true;
            this.SetMessage(Response.CommMessage);
            return this;
        }




        #region ============================================================================
        #endregion

        public static R Handle<R>(Func<R> function, Action<StdApiWebException> exceptionCusomization) {
            try {
                return function();
            }
            catch (StdApiWebException ex) {
                exceptionCusomization(ex);
                throw;
            }
        }



        public static void Handle<E>(Action function, Action<StdApiWebException> exceptionCusomization) {
            try {
                function();
            }
            catch (StdApiWebException ex) {
                exceptionCusomization(ex);
                throw;
            }
        }




        public static async Task<R> HandleAsync<R>(Func<Task<R>> function, Action<StdApiWebException> exceptionCusomization) {
            try {
                return await function();
            }
            catch (StdApiWebException ex) {
                exceptionCusomization(ex);
                throw;
            }
        }




        public static async Task HandleAsync<E>(Func<Task> function, Action<StdApiWebException> exceptionCusomization) {
            try {
                await function();
            }
            catch (StdApiWebException ex) {
                exceptionCusomization(ex);
                throw;
            }
        }




        #region ============================================================================
        #endregion
        // Static e Constants

        /// <summary>
        /// Wrapper para o contrutor privado com os mesmos parâmetros, para validar a construção,
        /// pois não é permitido criar um StdApiWebException a partir
        /// de uma <see cref="StdApiResponse"/> cujo <see cref="StdApiResponse.CommStatus"/> seja
        /// <see cref="StdApiResponse.CommunicationStatus.Success"/>
        /// </summary>
        public static StdApiWebException From(StdApiResponse response) {
            if (response.IsSuccess) return null;
            var ex = new StdApiWebException(response);
            return ex;
        }




        private const string _defaultMsg = "A chamada para um serviço externo falhou.";




        public static StdApiException CreateManually(object details) {
            return new StdApiException(HttpStatusCode.FailedDependency, _defaultMsg, details);
        }
    }








    public class StdApiWebExceptionDetails: StdApiWebExceptionDetails<object> { }
    public class StdApiWebExceptionDetails<T> {
        public int Status { get; set; }
        public string Message { get; set; }
        public T Details { get; set; }
        public Uri Uri { get; set; }
    }
}
