namespace Homework2.Domain.Common;

public abstract class DomainException : Exception
{
    protected DomainException(string? message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    protected DomainException(Exception innerException)
        : base(innerException?.Message, innerException)
    {
    }
}