using Core.Domain.Entities;

namespace Core.Application.Dtos.Responses;

public record TimeSlotResponse(
    Guid Id,
    Guid ClinicianId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    bool IsAvailable
)
{
    public static TimeSlotResponse FromEntity(TimeSlot timeSlot)
    {
        return new TimeSlotResponse(
            timeSlot.Id,
            timeSlot.ClinicianId,
            timeSlot.StartTime,
            timeSlot.EndTime,
            timeSlot.IsAvailable
        );
    }
}
