using Core.Domain.Enums;

namespace Core.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public Guid ClinicianId { get; set; }
    public Clinician Clinician { get; set; } = null!;
    
    public Guid TimeSlotId { get; set; }
    public TimeSlot TimeSlot { get; set; } = null!;
    
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    
    public string? Notes { get; set; }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled.");
        
        Status = AppointmentStatus.Cancelled;
        TimeSlot.Release();
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidOperationException("Only pending appointments can be confirmed.");
        
        Status = AppointmentStatus.Confirmed;
    }

    public void Reschedule(TimeSlot newTimeSlot)
    {
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot reschedule cancelled or completed appointments.");

        TimeSlot.Release();
        newTimeSlot.Reserve();
        
        TimeSlotId = newTimeSlot.Id;
        TimeSlot = newTimeSlot;
    }
}

