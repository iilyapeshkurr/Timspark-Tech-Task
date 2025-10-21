namespace IpLookupService.Exceptions;

public sealed class IPServiceNotAvailableException : Exception
{
    public IPServiceNotAvailableException(string message) : base(message) { }

    public IPServiceNotAvailableException(string message, Exception innerException)
        : base(message, innerException) { }
}