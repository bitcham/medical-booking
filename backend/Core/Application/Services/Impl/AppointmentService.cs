using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.Services.Impl;

public class AppointmentService(
    IAppointmentRepository appointmentRepository,
    ITimeSlotRepository timeSlotRepository,
    IPatientRepository patientRepository,
    IUnitOfWork unitOfWork
) : IAppointmentService
{
    public async Task<AppointmentResponse> CreateAsync(
        Guid patientId, 
        CreateAppointmentRequest request, 
        CancellationToken cancellationToken = default)
    {
        var timeSlot = await timeSlotRepository.GetByIdAsync(request.TimeSlotId, cancellationToken)
            ?? throw new TimeSlotNotFoundException();

        if (!timeSlot.IsAvailable)
        {
            throw new TimeSlotNotAvailableException();
        }

        var patient = await patientRepository.GetByIdAsync(patientId, cancellationToken)
            ?? throw new PatientNotFoundException();

        var appointment = new Appointment
        {
            PatientId = patientId,
            ClinicianId = timeSlot.ClinicianId,
            TimeSlotId = timeSlot.Id,
            Status = AppointmentStatus.Pending,
            Notes = request.Notes
        };

        timeSlot.Reserve();

        await appointmentRepository.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with all navigation properties
        var savedAppointment = await appointmentRepository.GetByIdAsync(appointment.Id, cancellationToken);

        return AppointmentResponse.FromEntity(savedAppointment!);
    }

    public async Task<AppointmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppointmentNotFoundException();
        return AppointmentResponse.FromEntity(appointment);
    }

    public async Task<IEnumerable<AppointmentResponse>> GetByPatientIdAsync(
        Guid patientId, 
        CancellationToken cancellationToken = default)
    {
        var appointments = await appointmentRepository.GetByPatientIdAsync(patientId, cancellationToken);
        return appointments.Select(AppointmentResponse.FromEntity);
    }

    public async Task<IEnumerable<AppointmentResponse>> GetByClinicianIdAsync(
        Guid clinicianId, 
        CancellationToken cancellationToken = default)
    {
        var appointments = await appointmentRepository.GetByClinicianIdAsync(clinicianId, cancellationToken);
        return appointments.Select(AppointmentResponse.FromEntity);
    }

    public async Task<AppointmentResponse> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppointmentNotFoundException();

        appointment.Cancel();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AppointmentResponse.FromEntity(appointment);
    }

    public async Task<AppointmentResponse> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppointmentNotFoundException();

        appointment.Confirm();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AppointmentResponse.FromEntity(appointment);
    }

    public async Task<AppointmentResponse> RescheduleAsync(
        Guid id, 
        RescheduleAppointmentRequest request, 
        CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppointmentNotFoundException();

        var newTimeSlot = await timeSlotRepository.GetByIdAsync(request.NewTimeSlotId, cancellationToken)
            ?? throw new TimeSlotNotFoundException();

        appointment.Reschedule(newTimeSlot);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AppointmentResponse.FromEntity(appointment);
    }
}
