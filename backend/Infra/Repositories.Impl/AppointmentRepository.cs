using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Impl;

public class AppointmentRepository(AppDbContext context) : IAppointmentRepository
{
    public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await context.Appointments.AddAsync(appointment, cancellationToken);
        return appointment;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Clinician).ThenInclude(c => c.User)
            .Include(a => a.TimeSlot)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Clinician).ThenInclude(c => c.User)
            .Include(a => a.TimeSlot)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.TimeSlot.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByClinicianIdAsync(Guid clinicianId, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Clinician).ThenInclude(c => c.User)
            .Include(a => a.TimeSlot)
            .Where(a => a.ClinicianId == clinicianId)
            .OrderByDescending(a => a.TimeSlot.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByTimeSlotIdAsync(Guid timeSlotId, CancellationToken cancellationToken = default)
    {
        return await context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Clinician).ThenInclude(c => c.User)
            .Include(a => a.TimeSlot)
            .FirstOrDefaultAsync(a => a.TimeSlotId == timeSlotId, cancellationToken);
    }
}
