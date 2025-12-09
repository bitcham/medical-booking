using Core.Domain.Entities;

namespace Core.Application.Services.Impl;

/// <summary>
/// Default TimeSlot generation strategy for MVP.
/// Generates slots from 08:00 to 18:00 with 30-minute intervals.
/// </summary>
public class DefaultTimeSlotStrategy : ITimeSlotGenerationStrategy
{
    private const int StartHour = 8;
    private const int EndHour = 18;
    private const int IntervalMinutes = 30;

    public IEnumerable<TimeSlot> GenerateSlots(Guid clinicianId, DateOnly date, TimeSpan offset)
    {
        for (var hour = StartHour; hour < EndHour; hour++)
        {
            for (var minute = 0; minute < 60; minute += IntervalMinutes)
            {
                var startTime = new DateTimeOffset(
                    date.Year, date.Month, date.Day, 
                    hour, minute, 0, offset);
                
                var endTime = startTime.AddMinutes(IntervalMinutes);

                yield return new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    ClinicianId = clinicianId,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsAvailable = true
                };
            }
        }
    }
}
