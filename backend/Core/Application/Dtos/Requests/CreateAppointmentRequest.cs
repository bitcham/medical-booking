namespace Core.Application.Dtos.Requests;

public record CreateAppointmentRequest(
    Guid TimeSlotId,
    string? Notes = null
);
