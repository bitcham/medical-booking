namespace Core.Application.Exceptions;

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException()
        : base("Email already in use") { }

    public DuplicateEmailException(string message)
        : base(message) { }

    public DuplicateEmailException(string message, Exception innerException)
        : base(message, innerException) { }
}