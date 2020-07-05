using System;
using System.Net;

namespace Client.Models
{
    public abstract class ApiResponse
    {
        public bool IsError { get; protected set; }
        public HttpStatusCode HttpStatusCode { get; }
        public string HttpReasonPhrase { get; }
        public string Raw { get; }
        public Exception Exception { get; protected set; }
        public ResponseError ResponseError { get; protected set; }
            = ResponseError.None;

        protected ApiResponse(
            HttpStatusCode statusCode,
            string raw,
            string reason = null)
        {
            HttpStatusCode = statusCode;
            HttpReasonPhrase = reason;
            Raw = raw;

            if (statusCode != HttpStatusCode.OK)
            {
                IsError = true;
                ResponseError = ResponseError.Http;
            }
        }

        protected ApiResponse(Exception ex)
        {
            IsError = true;
            Exception = ex;
            ResponseError = ResponseError.Exception;
        }

        public string Error
        {
            get
            {
                if (ResponseError == ResponseError.None) return string.Empty;
                if (ResponseError == ResponseError.Http) return HttpReasonPhrase;
                if (ResponseError == ResponseError.Exception) return Exception.Message;
                return Raw;
            }
        }
    }

    public class ApiResponse<T> :
        ApiResponse,
        IApiResponse<T>
    {
        public ApiResponse(Exception ex) : base(ex)
        {
        }

        public ApiResponse(
            HttpStatusCode statusCode,
            string raw,
            string reason = null) : base(statusCode, raw, reason)
        {
            if (ResponseError != ResponseError.None)
                return;

            try
            {
                Content = raw.ToTypedObject<T>();
            }
            catch (Exception ex)
            {
                IsError = true;
                Exception = ex;
                if (ex.InnerException != null)
                {
                    Exception = ex.InnerException;
                }
                ResponseError = ResponseError.Exception;
            }
        }

        public ApiResponse(
            T content,
            string raw,
            HttpStatusCode statusCode) : base(statusCode, raw)
        {
            Content = content;
        }

        public T Content { get; }
    }
}
