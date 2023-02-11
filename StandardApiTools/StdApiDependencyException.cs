using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StandardApiTools {
    public class StdApiWebException: Exception, IProduceStdApiResult {

        public static StdApiWebException From(StdApiResponse response, string message = null) {
            if (response.IsSuccess) return null;
            var ex = new StdApiWebException(response, message);
            return ex;
        }






        public StdApiWebException(string message, object details = null, Exception innerException = null)
        : base(message, innerException) {
            _isManuallyCreated = true;
            _result = new StdApiResult((int)HttpStatusCode.FailedDependency, message, details);
        }






        private StdApiWebException(StdApiResponse response, string message = null)
        : base(_defaultMessage, response.Exception) {
            Response = response;
            _additionResultMessage = message.TrimToNull();
        }






        public readonly StdApiResponse Response;
        public readonly List<SpecialCase> SpecialCases = new List<SpecialCase>();

        private readonly bool _isManuallyCreated;
        private StdApiResult _result;

        private readonly string _additionResultMessage;
        const string _defaultMessage = "A chamada para um serviço externo falhou.";






        public override string Message => Result.Message;
        public StdApiResult Result { get => _isManuallyCreated ? _result : GetResult(); }






        private StdApiResult GetResult() {
            if (Response?.IsSuccess == true) return null;
            SpecialCase? c = FindCase();
            if (!_isManuallyCreated) {
                var status = c != null ? c.Value.Status : (int)HttpStatusCode.FailedDependency;
                var message = c != null ? c.Value.Message?.Invoke(Response) : _defaultMessage.Join(_additionResultMessage);
                var details = c != null ? c.Value.Details?.Invoke(Response) : new {
                    Status = Response.HttpStatusCode != null
                        ? (int)Response.HttpStatusCode
                        : Response.CommStatusCode,
                    Description = Response.HttpStatusCode != null
                        ? Response.HttpStatusCode.ToString()
                        : Response.CommStatusSource?.ToString(),
                    Message = Response.CommMessage,
                    Data = Response.ContentAsString,
                    Uri = Response.RequestUri
                };
                return new StdApiResult(status, message, details);
            }
            else {
                var message = c != null ? c.Value.Message?.Invoke(null) : _result.Message;
                var details = c != null ? c.Value.Details?.Invoke(null) : _result.Data;
                return new StdApiResult((int)HttpStatusCode.FailedDependency, message, details);
            }
        }






        private SpecialCase? FindCase() {
            if (SpecialCases == null || SpecialCases.Count == 0) return null;
            var currentStatus = _isManuallyCreated
                ? _result.Status
                : Response?.CommStatusCode ?? (int?)Response.HttpStatusCode;
            foreach (var caso in SpecialCases) {
                var isMatch = caso.Status == currentStatus;
                //var isMatch = caso.Status == Response.CommStatusCode || caso.Status == (int?)Response.HttpStatusCode;
                var isConditionSatisfied = caso.Condition?.Invoke(Response) ?? true;
                if (isMatch && isConditionSatisfied) return caso;
            }
            return null;
        }






        public struct SpecialCase {
            public int Status;
            public Func<StdApiResponse, bool> Condition;
            public Func<StdApiResponse, string> Message;
            public Func<StdApiResponse, object> Details;
        }





        public static Task<R> HandleAsync<R>(Func<R> function, params Action<SpecialCase>[] caseBuilders) {
            return Task.Run(() => Handle(function, BuildCases(caseBuilders)));
        }




        public static Task<R> HandleAsync<R>(Func<R> function, params SpecialCase[] cases) {
            return Task.Run(() => Handle(function, cases));
        }




        public static R Handle<R>(Func<R> function, Action<SpecialCase>[] caseBuilders) {
            return Handle(function, BuildCases(caseBuilders));
        }




        public static R Handle<R>(Func<R> function, SpecialCase[] cases) {
            try {
                return function();
            }
            catch (Exception ex) {
                if (ex.Deaggregate() is StdApiWebException apiEx) {
                    apiEx.SpecialCases.AddRange(cases);
                    throw apiEx;
                }
                else throw;
            }
        }




        private static SpecialCase[] BuildCases(Action<SpecialCase>[] caseBuilders) {
            var array = new SpecialCase[caseBuilders.Length];
            for (int i = 0; i < caseBuilders.Length; i++) {
                array[i] = new SpecialCase();
                caseBuilders[i](array[i]);
            }
            return array;
        }
    }
}
