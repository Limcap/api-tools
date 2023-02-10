using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StdResponseTools {

    public class StdResponseException: Exception, IStdResponseResult {

        public static StdResponseException From(StdResponse response) {
            if (response.IsSuccess) return null;
            return new StdResponseException(response);
        }






        private StdResponseException(StdResponse response) {
            this.Response = response;
        }





        const string ExternalErrorMessage = "A chamada para um serviço externo falhou.";
        public readonly StdResponse Response;
        public readonly List<SpecialCase> SpecialCases;
        public StdResponseResult Result { get => GetResult(); }






        private StdResponseResult GetResult() {
            if (Response.IsSuccess) return null;
            int status = Response.HttpStatusCode == null ? (int)Response.HttpStatusCode : Response.CommStatusCode;
            string description = Response.HttpStatusCode == null ? Response.HttpStatusCode.ToString() : Response.CommStatusSource?.ToString();
            SpecialCase? c = FindCase();
            string message = c != null ? c?.Message?.Invoke(Response) : ExternalErrorMessage;
            object details = c != null ? c?.Details?.Invoke(Response) : new {
                Status = status,
                Description = description,
                Message = Response.CommMessage,
                Data = Response.ContentAsString,
                Uri = Response.RequestUri
            };
            return new StdResponseResult(status, message, details);
        }






        private SpecialCase? FindCase() {
            if (SpecialCases.Count == 0) return null;
            foreach (var caso in SpecialCases) {
                var isMatch = caso.Status == Response.CommStatusCode || caso.Status == (int?)Response.HttpStatusCode;
                var isConditionSatisfied = caso.Condition?.Invoke(Response) ?? true;
                if (isMatch && isConditionSatisfied) return caso;
            }
            return null;
        }






        public struct SpecialCase {
            public int Status;
            public Func<StdResponse, bool> Condition;
            public Func<StdResponse, string> Message;
            public Func<StdResponse, object> Details;
        }






        public static R SetSpecialCases<R>(
            Func<R> function,
            SpecialCase[] cases
        ) {
            try {
                return function();
            }
            catch (Exception ex) {
                if (ex.Deaggregate() is StdResponseException srex) {
                    srex.SpecialCases.AddRange(cases);
                    throw srex;
                }
                else throw;
            }
        }






        public static Task<R> SetSpecialCasesAsync<R>(
            Func<R> function,
            SpecialCase[] cases
        ) {
            return Task.Run(() => {
                try {
                    return function();
                }
                catch (Exception ex) {
                    if (ex.Deaggregate() is StdResponseException srex) {
                        srex.SpecialCases.AddRange(cases);
                        throw srex;
                    }
                    else throw;
                }
            });
        }
    }
}
