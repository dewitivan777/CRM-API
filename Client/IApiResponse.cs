using Client.Models;
using System;
using System.Net;

namespace Client
{
    public interface IApiResponse<T>
    {
        T Content { get; }
        string Error { get; }
        bool IsError { get; }
        HttpStatusCode HttpStatusCode { get; }
        string HttpReasonPhrase { get; }
        string Raw { get; }
        Exception Exception { get; }
        ResponseError ResponseError { get; }
    }
}
