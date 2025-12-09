namespace Core.Application.Exceptions;

public class TimeSlotNotAvailableException : Exception
{
    public TimeSlotNotAvailableException() : base("Time slot is not available.") { }
    public TimeSlotNotAvailableException(string message) : base(message) { }
}
