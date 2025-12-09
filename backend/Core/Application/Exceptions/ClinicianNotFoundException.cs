namespace Core.Application.Exceptions;

public class ClinicianNotFoundException : Exception
{
    public ClinicianNotFoundException() : base("Clinician not found.") { }
    public ClinicianNotFoundException(string message) : base(message) { }
}
