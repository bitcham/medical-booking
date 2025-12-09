namespace Core.Application.Exceptions;

public class TimeSlotNotFoundException : Exception
{
    public TimeSlotNotFoundException() : base("Time slot not found.") { }
    public TimeSlotNotFoundException(string message) : base(message) { }
}
