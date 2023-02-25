using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace StandardApiTools {
    public partial class StdApiWebException: StdApiException {

        /// <summary>
        /// Construtor privado pois é preciso validar primeiro a response pois
        /// só deve ser criado uma exceção se a resposta for de erro.
        /// <seealso cref="From(StdApiResponse, string)"/>
        /// </summary>
        private StdApiWebException(StdApiResponse response)
        : base(response.Exception) {
            Response = response;
            Info = new StdApiDataCollection();
            SpecialCases = new List<SpecialCase>(3);
            MessageParts = new List<string>(3);
            AddMessage(_defaultMsg);
        }




        public readonly StdApiResponse Response;
        public readonly List<SpecialCase> SpecialCases;
        //private readonly string _manualMsg;
        //public string AdditionalMessage { get => _additionalMsg; set => _additionalMsg = value.TrimToNull(); }
        //private string _additionalMsg;
        //private readonly object ManualContent;
        public readonly List<string> MessageParts;




        // Previne o acesso a esse mebro que não será usado.
        private new IDictionary Data { get; }
        //public StdApiDataCollection Info { get; }
        public override int StatusCode { get => CompileStatus(FindCase()); }
        public override object Content { get => CompileContent(FindCase()); }
        public override string Message { get => CompileMessage(FindCase()); }
        //public bool IsManuallyCreated { get => Response == null; }
        //private object Content { get => CompileContent(FindCase()); }




        public StdApiWebException AddMessage(string additionalMessage) {
            MessageParts.Add(additionalMessage.TrimToNull());
            return this;
        }




        # region ============================================================================
        #endregion
        // Relativo a conversão para resultado

        private int CompileStatus(SpecialCase? c) {
            var status = c != null
                ? c.Value.Status
                : (int)HttpStatusCode.FailedDependency;
            return status;
        }




        private string CompileMessage(SpecialCase? c) {
            var message = c != null
                ? c.Value.Message?.Invoke(Response)
                : string.Join(Environment.NewLine, MessageParts);
            return message;
        }




        private object CompileContent(SpecialCase? c) {
            if (c != null) return c.Value.Content?.Invoke(Response);
            return new {
                Status = Response.HttpStatus != null
                    ? (int)Response.HttpStatus
                    : (int)Response.CommStatus,
                Description = Response.HttpStatus != null
                    ? Response.HttpStatus.ToString()
                    : Response.CommStatus.ToString(),
                Message = Response.CommMessage,
                Data = Response.ContentAsString,
                Uri = Response.RequestUri
            };
        }




        public override StdApiErrorResult ToResult() {
            SpecialCase? c = FindCase();
            var result = new StdApiErrorResult(CompileStatus(c), CompileMessage(c), CompileContent(c));
            return result;
        }




        private SpecialCase? FindCase() {
            if (SpecialCases == null || SpecialCases.Count == 0) return null;
            var responseStatus = (int?)Response?.CommStatus ?? (int?)Response.HttpStatus;
            foreach (var caso in SpecialCases) {
                var isMatch = caso.Status == responseStatus;
                var isConditionSatisfied = caso.Condition?.Invoke(Response) ?? true;
                if (isMatch && isConditionSatisfied) return caso;
            }
            return null;
        }




        #region ============================================================================
        #endregion
        // Relativo a SpecialCase

        public StdApiWebException AddSpecialCases(params SpecialCase[] specialCases) {
            SpecialCases.AddRange(specialCases);
            return this;
        }




        public static R Handle<R>(Func<R> function, params Func<SpecialCase>[] caseFetchers) {
            return _HandleSync(function, _AssembleCases(caseFetchers));
        }

        public static R Handle<R>(Func<R> function, params Action<SpecialCase>[] caseMolders) {
            return _HandleSync(function, _AssembleCases(caseMolders));
        }

        public static R Handle<R>(Func<R> function, params SpecialCase[] cases) {
            return _HandleSync(function, cases);
        }

        public static Task<R> Handle<R>(Func<Task<R>> function, params Func<SpecialCase>[] caseFetchers) {
            return _HandleAsync(function, _AssembleCases(caseFetchers));
        }

        public static Task<R> Handle<R>(Func<Task<R>> function, params Action<SpecialCase>[] caseMolders) {
            return _HandleAsync(function, _AssembleCases(caseMolders));
        }

        public static Task<R> Handle<R>(Func<Task<R>> function, params SpecialCase[] cases) {
            return _HandleAsync(function, cases);
        }

        private static async Task<R> _HandleAsync<R>(Func<Task<R>> function, SpecialCase[] cases) {
            try {
                return await function();
            }
            catch (Exception ex) {
                _InjectCases(ex, cases);
                throw;
            }
        }

        private static R _HandleSync<R>(Func<R> function, SpecialCase[] cases) {
            try {
                return function();
            }
            catch (Exception ex) {
                _InjectCases(ex, cases);
                throw;
            }
        }




        private static void _InjectCases(Exception ex, SpecialCase[] cases) {
            if (ex.Deaggregate() is StdApiWebException apiEx)
                apiEx.SpecialCases.AddRange(cases);
        }




        private static SpecialCase[] _AssembleCases(Action<SpecialCase>[] caseMolders) {
            var array = new SpecialCase[caseMolders.Length];
            for (int i = 0; i < caseMolders.Length; i++) {
                array[i] = new SpecialCase();
                caseMolders[i](array[i]);
            }
            return array;
        }




        private static SpecialCase[] _AssembleCases(Func<SpecialCase>[] caseFetchers) {
            return caseFetchers.Select(c => c()).ToArray();
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




        /// <summary>
        /// Equivalente ao construtor público <see cref="StdApiWebException(string, object)"/>
        /// </summary>
        //public static StdApiWebException From(string message, object content = null) {
        //    return new StdApiWebException(message, content);
        //}




        #region ============================================================================
        #endregion
        // Contantes

        private const string _defaultMsg = "A chamada para um serviço externo falhou.";
    }
}
