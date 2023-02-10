﻿using System;
using static StandardResponseTools.ExternalServiceException;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StandardResponseTools {

    public class SRException: Exception, ISRReady {

        public SRException(int status, string message, object details=null)
        : base(message) {
            Status = status;
            Details = details;
        }



        public int Status { get; }
        public object Details { get; }



        public SRException(Exception originalException, string message = null, object details = null)
        : base(message??originalException.Message, originalException) {
            Status = 500;
            Details = details ?? originalException.ToString();
        }






        public static R Wrap<R>(
            Func<R> func,
            params (Type type ,Func<Exception,SRException> converter)[] handlers
        ) {
            try {
                return func();
            }
            catch (Exception ex) {
                foreach(var c in handlers) {
                    if (ex.GetType() == c.type) throw c.converter(ex);
                }
                throw new SRException(ex);
            }
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