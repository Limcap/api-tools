﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StandardApiTools {
    public partial class StdApiWebException: Exception, IProduceStdApiResult {

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
