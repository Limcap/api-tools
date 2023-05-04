﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading.Tasks;

namespace StandardApiTools {
    public partial class StdApiErrorResult: StdApiResult {

        public StdApiErrorResult(int status, string message, object details = null, object info = null, bool? suppressNullValues = null) {
            StatusCode = status;
            Message = message;
            Details = details;
            Info = info;
            SupressNullValues = suppressNullValues ?? SupressNullValuesFromResults;
        }




        public StdApiErrorResult(StdApiResponse response, string message)
        : this(response.StatusCode, message, response.ContentAsString) { }




        public StdApiErrorResult(StdApiResponse response)
        : this(response, response.CommMessage) { }




        public string Message { get; private set; }
        public object Details { get; private set; }
        public object Info { get; private set; }




        public bool SupressNullValues;
        public string CustomDataGroupName;




        public StdApiDataCollection GetInfoDataCollection() {
            Info ??= new StdApiDataCollection();
            return Info as StdApiDataCollection;
        }




        public object Compile() {
            var eo = new ExpandoObject();
            var eoc = (ICollection<KeyValuePair<string, object>>)eo;
            if (!SupressNullValues || Message != null)
                eoc.Add(new KeyValuePair<string, object>(MessageKeyName, Message));
            if (!SupressNullValues || Details != null)
                eoc.Add(new KeyValuePair<string, object>(DetailsKeyName, Details));
            var info = (Info as StdApiDataCollection)?.ToObject(SupressNullValues) ?? Info;
            if (!SupressNullValues || Info != null)
                eoc.Add(new KeyValuePair<string, object>(InfoKeyName, info));
            return eo;
        }




        public override async Task ExecuteResultAsync(ActionContext context) {
            CompiledResultObject = Compile();
            try {
                await base.ExecuteResultAsync(context);
            }
            catch (Exception ex) {
                throw new Exception("Error when executing AspNetCore.MVC result", ex);
            }
        }




        public static bool SupressNullValuesFromResults = false;
        public static string MessageKeyName = "message";
        public static string DetailsKeyName = "details";
        public static string InfoKeyName = "info";




        public static StdApiErrorResult CreateFrom(Exception ex, string message = null) {
            return StdApiException.CreateFrom(ex, message).ToResult();
            //if (ex == null) return null;
            //ex = ex.Deaggregate();
            //var result = ex is IProduceStdApiErrorResult pr
            //? pr.ToResult()
            //: StdApiException.CreateFrom(ex, "Ocorreu um erro não identificado durante o processamento.").ToResult();
            //return result;
        }
    }
}
