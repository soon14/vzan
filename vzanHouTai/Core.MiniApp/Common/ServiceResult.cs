using System;
using System.Collections.Generic;

namespace Core.MiniApp
{
    public class ServiceResult
    {
        public ServiceResult() { }

        public ServiceResult(string msg)
        {
            Message = msg;
        }

        public bool Success => ResultCode == ServiceResultCode.Succeed;

        public int ResultCode;
        public string Message;
        public Dictionary<string, object> Data = new Dictionary<string, object>();
    }

    public static class ServiceResultExtensions
    {
        #region IsSucceed

        public static ServiceResult IsSucceed(this ServiceResult svr)
        {
            svr.ResultCode = ServiceResultCode.Succeed;
            return svr;
        }

        public static ServiceResult IsSucceed(this ServiceResult svr, string msg)
        {
            svr.ResultCode = ServiceResultCode.Succeed;
            svr.Message = msg;
            return svr;
        }

        public static ServiceResult IsSucceed(this ServiceResult svr, Exception ex)
        {
            svr.ResultCode = ServiceResultCode.Succeed;
            SetMessage(svr, ex);
            return svr;
        }

        #endregion

        #region IsFailed

        public static ServiceResult IsFailed(this ServiceResult svr)
        {
            svr.ResultCode = ServiceResultCode.Failed;
            return svr;
        }

        public static ServiceResult IsFailed(this ServiceResult svr, string msg)
        {
            svr.ResultCode = ServiceResultCode.Failed;
            svr.Message = msg;
            return svr;
        }

        public static ServiceResult IsFailed(this ServiceResult svr, Exception ex)
        {
            svr.ResultCode = ServiceResultCode.Failed;
            SetMessage(svr, ex);
            return svr;
        }

        #endregion

        private static void SetMessage(ServiceResult svr, Exception ex)
        {
            svr.Message = ex.InnerException == null ?
                ex.ToString() :
                ex + "\r\nInnerException:\r\n" + ex.InnerException;
        }

        public static void Set(this ServiceResult svr, string name, object value)
        {
            svr.Data[name] = value;
        }

        public static T Get<T>(this ServiceResult svr, string key) where T : class
        {
            if (svr.Data.ContainsKey(key))
                return svr.Data[key] as T;
            return null;
        }

        public static bool TryGet(this ServiceResult svr, string name, out object value)
        {
            return svr.Data.TryGetValue(name, out value);
        }
    }

    internal static class ServiceResultCode
    {
        internal static readonly int Succeed = 0;
        internal static readonly int Failed = 1;
    }
}