using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StandardApiTools {
    public class StdApiDependencyException: Exception, IProduceStdApiResult {

        public StdApiDependencyException(string message, object details=null, Exception innerException=null)
        : base(message,innerException) {
            _Result = new StdResult((int)HttpStatusCode.FailedDependency, message, details);
        }






        public static StdApiDependencyException From(StdApiResponse response) {
            if (response.IsSuccess) return null;
            return new StdApiDependencyException(response);
        }






        private StdApiDependencyException(StdApiResponse response)
        : base(ExternalErrorMessage, response.Exception) {
            this.Response = response;
        }





        private readonly bool IsManuallyCreated;
        const string ExternalErrorMessage = "A chamada para um serviço externo falhou.";
        public readonly StdApiResponse Response;
        public readonly List<SpecialCase> SpecialCases;
        public StdResult Result { get => IsManuallyCreated ? _Result : GetResult(); }
        private StdResult _Result;
        //public int Status => Result.Status;
        public override string Message => Result.Message;
        //public object Details => Result.Data;






        private StdResult GetResult() {
            if (Response?.IsSuccess == true) return null;
            SpecialCase? c = FindCase();
            if (!IsManuallyCreated) {
                var status = c != null ? c.Value.Status : (int)HttpStatusCode.FailedDependency;
                var message = c != null ? c.Value.Message?.Invoke(Response) : ExternalErrorMessage;
                var details = c != null ? c.Value.Details?.Invoke(Response) : new {
                    Status = Response.HttpStatusCode == null
                        ? (int)Response.HttpStatusCode
                        : Response.CommStatusCode,
                    Description = Response.HttpStatusCode == null
                        ? Response.HttpStatusCode.ToString()
                        : Response.CommStatusSource?.ToString(),
                    Message = Response.CommMessage,
                    Data = Response.ContentAsString,
                    Uri = Response.RequestUri
                };
                return new StdResult(status, message, details);
            }
            else {
                var message = c != null ? c.Value.Message?.Invoke(null) : _Result.Message;
                var details = c != null ? c.Value.Details?.Invoke(null) : _Result.Data;
                return new StdResult((int)HttpStatusCode.FailedDependency, message, details);
            }
        }






        private SpecialCase? FindCase() {
            if (SpecialCases.Count == 0) return null;
            var currentStatus = IsManuallyCreated
                ? _Result.Status
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






        public static R SetSpecialCases<R>(
            Func<R> function,
            SpecialCase[] cases
        ) {
            try {
                return function();
            }
            catch (Exception ex) {
                if (ex.Deaggregate() is StdApiDependencyException srex) {
                    srex.SpecialCases.AddRange(cases);
                    throw srex;
                }
                else throw;
            }
        }






        public static Task<R> SetSpecialCasesAsync<R>(
            Func<R> function,
            params Action<SpecialCase>[] caseBuilders
        ) {
            return Task.Run(() => {
                try {
                    return function();
                }
                catch (Exception ex) {
                    if (ex.Deaggregate() is StdApiDependencyException srex) {
                        foreach (var propertySetter in caseBuilders) {
                            var c = new SpecialCase();
                            propertySetter(c);
                            srex.SpecialCases.Add(c);
                        }
                        throw;
                    }
                    else throw;
                }
            });
        }
    }
}
