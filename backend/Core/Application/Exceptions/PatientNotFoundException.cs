namespace Core.Application.Exceptions;

public class PatientNotFoundException : Exception
{
    public PatientNotFoundException() : base("Patient not found.") { }
    public PatientNotFoundException(string message) : base(message) { }
}
