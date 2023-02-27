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
            //content = response.ContentAsString;
            //statusCode = (int) HttpStatusCode.FailedDependency;
            this.AddMessage(_defaultMsg);
        }




        public readonly StdApiResponse Response;



        public bool UseResponseStatusCode { get; set; } = false;
        public override int StatusCode {
            get => UseResponseStatusCode ? Response.StatusCode : (int)HttpStatusCode.FailedDependency;
        }
        public override object Content {
            get => content != null ? content : CompileContent();
            set => content = value;
        }




        //migrado para extensao
        //public StdApiWebException SetMessage(string message) {
        //    MessageParts.Clear();
        //    MessageParts.Add(message.TrimToNull());
        //    return this;
        //}




        //migrado para extensao
        //public new StdApiWebException AddMessage(string message) {
        //    MessageParts.Add(message.TrimToNull());
        //    return this;
        //}




        //migrado para extensao
        //public StdApiWebException SetContent(object content) {
        //    this.content = content;
        //    return this;
        //}



        //migrado para extensao
        //public StdApiWebException DesserializeContent<T>(JsonSerializerOptions opt = null) {
        //    if(content is string str){
        //        try {
        //            content = JsonSerializer.Deserialize<T>(str, opt);
        //        }
        //        catch (Exception e) {
        //            Info.Add(
        //                "Erro de desserialização",
        //                "O conteúdo está apresentado na forma inicial, pois não foi possível desserializá-lo para o formato definido."
        //            );
        //            content = JsonSerializer.Deserialize<object>(str);
        //        }
        //    }
        //    return this;
        //}



        //migrado para extensao
        //public new StdApiWebException AddInfo(string key, object value) {
        //    Info.Add(key, value);
        //    return this;
        //}




        #region ============================================================================
        #endregion
        // Relativo a conversão para resultado

        private object CompileContent() {
            return new {
                Status = Response.HttpStatus != null
                    ? (int)Response.HttpStatus
                    : (int)Response.CommStatus,
                Description = Response.HttpStatus != null
                    ? Response.HttpStatus.ToString()
                    : Response.CommStatus.ToString(),
                Message = Response.CommMessage,
                Content = Response.ContentAsString,
                Uri = Response.RequestUri
            };
        }




        //public new StdApiException SetCustomResultType<T>()
        //where T : IErrorToResultConverter<StdApiWebException>, new() {
        //    var t = new T { Exception = this };
        //    CustomResultMaker = t;
        //    return this;
        //}



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
        // Static

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




        #region ============================================================================
        #endregion
        // Contantes

        private const string _defaultMsg = "A chamada para um serviço externo falhou.";
    }
}
