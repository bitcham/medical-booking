using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Entities;

public class TimeSlot : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid ClinicianId { get; set; }
    public Clinician Clinician { get; set; } = null!;
    
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    [Timestamp]
    public uint RowVersion { get; set; }

    public void Reserve()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Time slot is not available.");
        IsAvailable = false;
    }

    public void Release()
    {
        IsAvailable = true;
    }
}

