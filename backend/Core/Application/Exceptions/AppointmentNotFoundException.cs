namespace Core.Application.Exceptions;

public class AppointmentNotFoundException : Exception
{
    public AppointmentNotFoundException() : base("Appointment not found.") { }
    public AppointmentNotFoundException(string message) : base(message) { }
}
