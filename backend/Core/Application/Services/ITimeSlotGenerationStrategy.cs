using Core.Domain.Entities;

namespace Core.Application.Services;

public interface ITimeSlotGenerationStrategy
{
    IEnumerable<TimeSlot> GenerateSlots(Guid clinicianId, DateOnly date, TimeSpan offset);
}
