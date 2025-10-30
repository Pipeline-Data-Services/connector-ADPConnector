using System;
using System.IO;

namespace Connector.Client;

public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public Stream? Content { get; init; }

    public ApiException() : base()
    {
    }

    public ApiException(string message) : base(message)
    {
    }

    public ApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}