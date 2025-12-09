using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;

namespace Core.Application.Services.Impl;

public class TimeSlotService(
    ITimeSlotRepository timeSlotRepository,
    ITimeSlotGenerationStrategy generationStrategy,
    IUnitOfWork unitOfWork
) : ITimeSlotService
{
    public async Task<IEnumerable<TimeSlotResponse>> GenerateSlotsAsync(
        Guid clinicianId, 
        GenerateTimeSlotsRequest request, 
        CancellationToken cancellationToken = default)
    {
        var offset = request.Offset ?? TimeSpan.Zero;
        var slots = generationStrategy.GenerateSlots(clinicianId, request.Date, offset).ToList();

        await timeSlotRepository.AddRangeAsync(slots, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return slots.Select(TimeSlotResponse.FromEntity);
    }

    public async Task<IEnumerable<TimeSlotResponse>> GetByClinicianIdAsync(
        Guid clinicianId, 
        CancellationToken cancellationToken = default)
    {
        var slots = await timeSlotRepository.GetByClinicianIdAsync(clinicianId, cancellationToken);
        return slots.Select(TimeSlotResponse.FromEntity);
    }

    public async Task<IEnumerable<TimeSlotResponse>> GetAvailableByClinicianIdAsync(
        Guid clinicianId, 
        DateOnly date, 
        CancellationToken cancellationToken = default)
    {
        var slots = await timeSlotRepository.GetAvailableByClinicianIdAsync(clinicianId, date, cancellationToken);
        return slots.Select(TimeSlotResponse.FromEntity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var slot = await timeSlotRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new TimeSlotNotFoundException();

        // Soft delete handled by DbContext
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
