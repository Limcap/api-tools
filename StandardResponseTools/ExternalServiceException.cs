using RestSharp;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CustomCaseBuilder = System.ValueTuple<int, System.Func<bool, StandardResponseTools.CachedResponse>, System.Func<string, StandardResponseTools.CachedResponse>, System.Func<object, StandardResponseTools.CachedResponse>>;

namespace StandardResponseTools {

    public class ExternalServiceException: Exception, ISRReady {

        private ExternalServiceException(int status, string message, object details) {
            Status = status;
            _Message = message;
            Details = details;
        }






        public ExternalServiceException(object details, string message = null) {
            Status = (int)HttpStatusCode.FailedDependency;
            _Message = message != null
                ? DefaultMessage + Environment.NewLine + message
                : DefaultMessage;
            Details = details;
        }






        public ExternalServiceException(WebException ex, string message = null) {
            Status = (int)HttpStatusCode.FailedDependency;
            _Message = message != null
                ? DefaultMessage + Environment.NewLine + message
                : DefaultMessage;
            Details = DefaultDetailsFormat(new CachedResponse(ex));
        }






        public ExternalServiceException(WebException ex, params CustomCase[] customCases) {
            var cresp = new CachedResponse(ex);
            CustomCase? c = FindCase(cresp, customCases);
            Status = c != null ? c.Value.Status : (int)HttpStatusCode.FailedDependency;
            _Message = c != null ? c.Value.Message?.Invoke(cresp) : DefaultMessage;
            Details = c != null ? c.Value.Details?.Invoke(cresp) : DefaultDetailsFormat(cresp);
        }
        //public ExternalServiceException(WebException ex, params CustomCase[] casos) {
        //    bool foundCase = false;
        //    var resp = new CachedResponse(ex);
        //    if(casos.Length>0) {
        //        foreach (var caso in casos) {
        //            int status = resp.HttpStatusCode != null ? (int)resp.HttpStatusCode : (int)ex.Status;
        //            var conditionSatisfied = caso.Condition?.Invoke(resp) ?? true;
        //            //var isAnyProtocolError = ex.Status == WebExceptionStatus.ProtocolError && Enum.IsDefined(typeof(HttpStatusCode), caso.Status);
        //            var statusMach1 = caso.Status == 7 && Enum.IsDefined(typeof(HttpStatusCode), status);
        //            var statusMatch2 = caso.Status == status; 
        //            if (conditionSatisfied && (statusMach1 || statusMatch2)) {
        //                foundCase = true;
        //                Status = caso.Status;
        //                _Message = caso.Message?.Invoke(resp);
        //                Details = caso.Details?.Invoke(resp);
        //            }
        //        }
        //    }
        //    if(!foundCase) {
        //        Status = (int) HttpStatusCode.FailedDependency;
        //        _Message = DefaultMessage;
        //        Details = DefaultDetailsFormat(resp);
        //    }
        //}






        public int Status { get; }
        public object Details { get; }
        public override string Message => _Message;
        private string _Message;
        const string DefaultMessage = "A chamada para um serviço externo falhou.";






        public static void Assert(IRestResponse resp, params CustomCase[] customCases) {
            if (isSucess(resp)) return;
            int status = resp.ResponseStatus == ResponseStatus.Completed ? (int)resp.StatusCode : (int)resp.ResponseStatus;
            string description = resp.ResponseStatus == ResponseStatus.Completed ? resp.StatusCode.ToString() : resp.ResponseStatus.ToString();
            var cresp = new CachedResponse(resp);
            CustomCase? caso = FindCase(cresp, customCases);
            string message = caso != null ? caso?.Message?.Invoke(cresp) : DefaultMessage;
            object details = caso != null ? caso?.Details?.Invoke(cresp) : new {
                Status = status,
                Description = description,
                Message = resp.ErrorMessage,
                Data = cresp.ContentAsString,
                Uri = cresp.Uri
            };
            throw new ExternalServiceException(status, message, details);



            static bool isSucess(IRestResponse resp) => resp.ResponseStatus == ResponseStatus.Completed && (int)resp.StatusCode < 300;



            //CustomCase? findCase() {
            //    if (casos.Length == 0) return null;
            //    int status = resp.ResponseStatus == ResponseStatus.Completed ? (int)resp.StatusCode : (int)resp.ResponseStatus;
            //    foreach (var caso in casos) {
            //        var conditionSatisfied = caso.Condition?.Invoke(cresp) ?? true;
            //        var statusMatch1 = caso.Status == ((int)ResponseStatus.Completed) && Enum.IsDefined(typeof(HttpStatusCode), status);
            //        var statusMatch2 = caso.Status == status;
            //        if (conditionSatisfied && (statusMatch1 || statusMatch2)) return caso;
            //    }
            //    return null;
            //}
        }





        static CustomCase? FindCase(CachedResponse cresp, CustomCase[] casos) {
            if (casos.Length == 0) return null;
            foreach (var caso in casos) {
                var isMatch = caso.Status == cresp.CommStatusCode || caso.Status == (int?)cresp.HttpStatusCode;
                var isConditionSatisfied = caso.Condition?.Invoke(cresp) ?? true;
                if (isMatch && isConditionSatisfied) return caso;
            }
            return null;
        }






        object DefaultDetailsFormat(CachedResponse r) {
            return new {
                Status = r.HttpStatusCode != null ? (int)r.HttpStatusCode : r.CommStatusCode,
                Description = r.HttpStatusCode != null ? r.HttpStatusCode.ToString() : r.CommStatusSource?.ToString(),
                Message = r.CommMessage,
                Data = r.ContentAsString,
                Uri = r.Uri,
            };
        }

        //object DefaultDetails(WebException ex, CachedResponse? cr = null){
        //    var r = cr ?? new CachedResponse(ex);
        //    return new {
        //        Status = r.HttpStatusCode == null ? (int)ex.Status : (int)r.HttpStatusCode,
        //        Description = r.HttpStatusCode == null ? ex.Status.ToString() : r.HttpStatusCode.ToString(),
        //        Message = ex.Message,
        //        Data = r.ContentAsString,
        //        Uri = r.Uri,
        //    };
        //}




        //public class WebExceptionDetails {
        //    public WebExceptionDetails(WebException ex) {
        //        CachedResponse = new CachedResponse(ex.Response);
        //        var r = CachedResponse;
        //        Status = r.Status == null ? (int)ex.Status : (int)r.Status;
        //        Description = r.Status == null ? ex.Status.ToString() : r.Status.ToString();
        //        Message = ex.Message;
        //        Data = r.ContentAsString;
        //        Uri = r.Uri;
        //    }
        //    public CachedResponse CachedResponse;
        //    public int Status { get; set; }
        //    public string Description { get; set; }
        //    public string Message { get; set; }
        //    public object Data { get; set; }
        //    public string Uri { get; set; }
        //}






        public struct CustomCase {
            public int Status;
            public CaseCondition Condition;
            public MessageBuilder Message;
            public DetailsBuilder Details;

            /// <summary>
            /// Condição necessária para o caso ser aplicado.
            /// </summary>
            /// <param name="hostData">Dados retornados pelo host.</param>
            public delegate bool CaseCondition(CachedResponse hostData);

            /// <summary>
            /// Contrói a mensagem da exceção.
            /// </summary>
            /// <param name="hostData">Dados retornados pelo host.</param>
            /// <returns></returns>
            public delegate string MessageBuilder(CachedResponse hostData);

            /// <summary>
            /// Contrói os detalhes da exceção.
            /// </summary>
            /// <param name="hostData">Dados retornados pelo host.</param>
            /// <returns></returns>
            public delegate object DetailsBuilder(CachedResponse hostData);

            public CustomCase(int status, CaseCondition condition, MessageBuilder message, DetailsBuilder details) {
                this.Status = status;
                this.Condition = condition;
                this.Message = message;
                this.Details = details;
            }
        }






        public static Task<R> WrapAsync<R>(
            Func<R> func,
            //params CustomCaseBuilder[] cases2,
            params (int status,
                CustomCase.CaseCondition condition,
                CustomCase.MessageBuilder message,
                CustomCase.DetailsBuilder details
            )[] cases
        ) {
            return Task.Run(() => {
                try {
                    return func();
                }
                catch (Exception ex) {
                    if (ex.Pop() is WebException wx) {
                        throw new ExternalServiceException(wx,
                            cases.Select(c => new CustomCase() {
                                Status = c.status,
                                Condition = c.condition,
                                Message = c.message,
                                Details = c.details
                            }).ToArray()
                        );
                    }
                    else throw;
                }
            });
        }
    }











    /*
    public class ServicoExternoException: Exception, IApiReadyException {

        public ServicoExternoException(int code, string message, Exception innerException = null)
        : this(code, message, null, innerException) { }



        public ServicoExternoException(int code, string message, object data, Exception innerException = null)
        : base(message, innerException) {
            Status = code;
            Data = data;
        }



        public ServicoExternoException(WebException ex, params CasoEspecial[] filtros) {
            bool filtroAplicado = false;
            var info = new WebResponseInfo(ex);
            if (filtros.Length > 0) {
                foreach (var filtro in filtros) {
                    bool condicaoAtendida = filtro.Condicao?.Invoke(info.HttpMessage) ?? true;
                    if (info.HttpStatus == filtro.Status && condicaoAtendida) {
                        filtroAplicado = true;
                        Status = (int)filtro.Status;
                        _Message = filtro.Mensagem ?? ex.Message; // MensagemPadrao(filtro.Status);
                        Data = (filtro.IncluirDadosDoHost ?? true) ? info.HttpMessage : null;
                    }
                }
            }
            if (!filtroAplicado) {
                Status = (int)HttpStatusCode.FailedDependency;
                _Message = $"A chamada para um serviço externo falhou.";
                Data = new {
                    Status = info.HttpStatus == null ? (int)ex.Status : (int)info.HttpStatus,
                    StatusDescription = info.HttpStatus == null ? ex.Status.ToString() : info.HttpStatus.ToString(),
                    Message = ex.Message,
                    Details = info.HttpMessage,
                    Uri = info.Uri
                };
            }
        }



        public int Status { get; private set; }
        public object Data { get; }
        public override string Message => _Message;
        private string _Message;



        public JsonResult RespostaPadrao => this;



        string MensagemPadrao(HttpStatusCode status) {
            return $"Um serviço externo necessário falhou com o seguinte retorno: ({(int)status}) {status}";
        }




        public static implicit operator RespostaPadrao(ServicoExternoException e) {
            return new RespostaPadrao(e.Status, e.Message, e.Data);
        }



        public static implicit operator JsonResult(ServicoExternoException e) => e;



        public struct CasoEspecial {
            public HttpStatusCode Status;
            public Condicional Condicao;
            public ContrutorDeMensagem Dados;
            public string Mensagem;
            public bool? IncluirDadosDoHost;

            /// <summary>
            /// Indica se o caso deve ser aplicado baseado no sucesso da condição.
            /// </summary>
            /// <param name="mensagemDoHost">Mensagem retornada pelo host.</param>
            public delegate bool Condicional(string mensagemDoHost);

            /// <summary>
            /// Transforma a mensagem do host para ser incluída na resposta.
            /// </summary>
            /// <param name="mensagemDoHost"></param>
            /// <returns></returns>
            public delegate string ContrutorDeMensagem(string mensagemDoHost);
        }
    }
    */
}