using System.Runtime.Serialization;

namespace OrderStream.Domain.Exceptions;

[Serializable]
public class DomainException : Exception
{
    public DomainException() { }

    public DomainException(string message) : base($"Domain Exception: \"{message}\" throws from Domain Layer.") { }

    public DomainException(string message, Exception innerException) : base($"Domain Exception: \"{message}\" throws from Domain Layer.", innerException) { }
}
