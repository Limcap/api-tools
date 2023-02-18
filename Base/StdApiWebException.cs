using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static StandardApiTools.StdApiWebException.SpecialCase;

namespace StandardApiTools {
    public partial class StdApiWebException: Exception, IProduceStdApiResult {

        /// <summary>
        /// Wrapper para o contrutor privado com os mesmos parâmetros, para validar a construção,
        /// pois não é permitido criar um StdApiWebException a partir
        /// de uma <see cref="StdApiResponse"/> cujo <see cref="StdApiResponse.CommStatus"/> seja
        /// <see cref="StdApiResponse.CommunicationStatus.Success"/>
        /// </summary>
        public static StdApiWebException From(StdApiResponse response, string additionalMessage = null, object additionalInfo = null) {
            if (response.IsSuccess) return null;
            var ex = new StdApiWebException(response, additionalMessage, additionalInfo);
            return ex;
        }




        /// <summary>
        /// Equivalente ao construtor público <see cref="StdApiWebException(string, object, object)"/>
        /// </summary>
        public static StdApiWebException From(string message, object content = null, object additionalInfo = null) {
            return new StdApiWebException(message, content, additionalInfo);
        }




        /// <summary>
        /// Constroi uma exceção a partir dos parametros informados, cujo status sempre será
        /// <see cref="HttpStatusCode.FailedDependency"/> se algum <see cref="SpecialCase"/>
        /// não for aplicado.
        /// </summary>
        public StdApiWebException(string message, object content = null, object info = null)//, Exception innerException = null
        : base(message) {
            _manualMessage = message;
            _manualContent = content;
            AdditionalInfo = info;
        }




        /// <summary>
        /// Construtor privado pois é preciso validar primeiro a response pois
        /// só deve ser criado uma exceção se a resposta for de erro.
        /// <seealso cref="From(StdApiResponse, string)"/>
        /// </summary>
        private StdApiWebException(StdApiResponse response, string additionalMessage = null, object additionalInfo = null)
        : base(_defaultMessage, response.Exception) {
            Response = response;
            AdditionalMessage = additionalMessage;
            AdditionalInfo = additionalInfo;
        }




        public readonly StdApiResponse Response;
        public readonly List<SpecialCase> SpecialCases = new List<SpecialCase>();
        private bool IsManuallyCreated => Response == null;

        public override string Message => IsManuallyCreated ? _manualMessage.Join(_addMsg) : _defaultMessage.Join(_addMsg);
        const string _defaultMessage = "A chamada para um serviço externo falhou.";
        private readonly string _manualMessage;
        
        public string AdditionalMessage { get => _addMsg; set => _addMsg = value.TrimToNull(); }
        private string _addMsg;

        public object AdditionalInfo { get; set; }
        private object _manualContent;




        public StdApiResult GetResult() {
            if (!IsManuallyCreated && !Response.IsSuccess) return null;
            SpecialCase? c = FindCase();
            if (IsManuallyCreated) return GetManualResult(c);
            return GetResultFromResponse(c);
        }




        private StdApiResult GetResultFromResponse(SpecialCase? c) {
            var status = c != null ? c.Value.Status : (int)HttpStatusCode.FailedDependency;
            var message = c != null ? c.Value.Message?.Invoke(Response) : Message;
            var content = c != null ? c.Value.Content?.Invoke(Response) : new {
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
            return new StdApiResult(status, message, content, AdditionalInfo);
        }




        private StdApiResult GetManualResult(SpecialCase? c) {
            var message = c != null ? c.Value.Message?.Invoke(null) : Message;
            var content = c != null ? c.Value.Content?.Invoke(null) : _manualContent;
            return new StdApiResult((int)HttpStatusCode.FailedDependency, message, content, AdditionalInfo);
        }




        private SpecialCase? FindCase() {
            if (SpecialCases == null || SpecialCases.Count == 0) return null;
            var currentStatus = IsManuallyCreated
                ? (int)HttpStatusCode.FailedDependency
                : (int?)Response?.CommStatus ?? (int?)Response.HttpStatus;
            foreach (var caso in SpecialCases) {
                var isMatch = caso.Status == currentStatus;
                var isConditionSatisfied = caso.Condition?.Invoke(Response) ?? true;
                if (isMatch && isConditionSatisfied) return caso;
            }
            return null;
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
    }
}
