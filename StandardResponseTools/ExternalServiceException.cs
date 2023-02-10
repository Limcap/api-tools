using RestSharp;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StandardResponseTools {

    public class ExternalServiceException: Exception, ISRReady {



        private ExternalServiceException(int status, string message, object details) {
            Status = status;
            _Message = message;
            Details = details;
        }






        public ExternalServiceException(object details) {
            Status = (int)HttpStatusCode.FailedDependency;
            _Message = DefaultMessage;
            Details = details;
        }






        public ExternalServiceException(WebException ex, params CustomCase[] casos) {
            bool foundCase = false;
            var resp = new CachedResponse(ex.Response);
            if(casos.Length>0) {
                foreach (var caso in casos) {
                    int status = resp.Status != null ? (int)resp.Status : (int)ex.Status;
                    var conditionSatisfied = caso.Condition?.Invoke(resp) ?? true;
                    //var isAnyProtocolError = ex.Status == WebExceptionStatus.ProtocolError && Enum.IsDefined(typeof(HttpStatusCode), caso.Status);
                    var statusMach1 = caso.Status == 7 && Enum.IsDefined(typeof(HttpStatusCode), status);
                    var statusMatch2 = caso.Status == status; 
                    if (conditionSatisfied && (statusMach1 || statusMatch2)) {
                        foundCase = true;
                        Status = caso.Status;
                        _Message = caso.Message?.Invoke(resp);
                        Details = caso.Details?.Invoke(resp);//??resp.ContentAsString;
                    }
                }
            }
            if(!foundCase) {
                Status = (int) HttpStatusCode.FailedDependency;
                _Message = DefaultMessage;
                Details = new {
                    Status = resp.Status == null ? (int)ex.Status : (int)resp.Status,
                    Description = resp.Status == null ? ex.Status.ToString() : resp.Status.ToString(),
                    //Message = MensagemPadrao,
                    Data = resp.ContentAsString,
                    Uri = resp.Uri
                };
            }
        }






        public int Status { get; }
        public object Details { get; }
        public override string Message => _Message;
        private string _Message;
        const string DefaultMessage = "A chamada para um serviço externo falhou.";






        public static void Assert(IRestResponse resp, params CustomCase[] casos) {
            if (isSucess(resp)) return;
            int status = resp.ResponseStatus == ResponseStatus.Completed ? (int)resp.StatusCode : (int)resp.ResponseStatus;
            string description = resp.ResponseStatus == ResponseStatus.Completed ? resp.StatusCode.ToString() : resp.ResponseStatus.ToString();
            var cachedResponse = new CachedResponse(resp);
            CustomCase? caso = findCase();
            string message = caso != null ? caso?.Message?.Invoke(cachedResponse) : DefaultMessage;
            object details = caso != null ? caso?.Details?.Invoke(cachedResponse) : new {
                Status = status,
                Description = description,
                Data = cachedResponse.ContentAsString,
                Uri = cachedResponse.Uri
            };
            throw new ExternalServiceException(status, message, details);



            static bool isSucess(IRestResponse resp) => resp.ResponseStatus == ResponseStatus.Completed && (int)resp.StatusCode < 300;



            CustomCase? findCase() {
                if (casos.Length == 0) return null;
                int status = resp.ResponseStatus == ResponseStatus.Completed ? (int)resp.StatusCode : (int)resp.ResponseStatus;
                foreach (var caso in casos) {
                    var conditionSatisfied = caso.Condition?.Invoke(cachedResponse) ?? true;
                    var statusMatch1 = caso.Status == ((int)ResponseStatus.Completed) && Enum.IsDefined(typeof(HttpStatusCode), status);
                    var statusMatch2 = caso.Status == status;
                    if (conditionSatisfied && (statusMatch1 || statusMatch2)) return caso;
                }
                return null;
            }
        }






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