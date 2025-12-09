namespace Core.Application.Dtos.Requests;

public record RescheduleAppointmentRequest(
    Guid NewTimeSlotId
);
