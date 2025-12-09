using Core.Application.Dtos.Requests;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Application.Services;
using Core.Application.Services.Impl;
using Core.Domain.Entities;
using Moq;

namespace backend.Tests.Services;

public class TimeSlotServiceTests
{
    private readonly Mock<ITimeSlotRepository> _timeSlotRepositoryMock;
    private readonly Mock<ITimeSlotGenerationStrategy> _strategyMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TimeSlotService _sut;

    public TimeSlotServiceTests()
    {
        _timeSlotRepositoryMock = new Mock<ITimeSlotRepository>();
        _strategyMock = new Mock<ITimeSlotGenerationStrategy>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new TimeSlotService(
            _timeSlotRepositoryMock.Object,
            _strategyMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task GenerateSlotsAsync_ShouldCallStrategyAndSaveSlots()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var request = new GenerateTimeSlotsRequest(DateOnly.FromDateTime(DateTime.Today));
        var generatedSlots = new List<TimeSlot>
        {
            CreateTimeSlot(clinicianId),
            CreateTimeSlot(clinicianId),
            CreateTimeSlot(clinicianId)
        };

        _strategyMock
            .Setup(x => x.GenerateSlots(clinicianId, request.Date, It.IsAny<TimeSpan>()))
            .Returns(generatedSlots);

        // Act
        var result = await _sut.GenerateSlotsAsync(clinicianId, request);

        // Assert
        Assert.Equal(3, result.Count());
        _timeSlotRepositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeSlot>>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByClinicianIdAsync_ShouldReturnTimeSlots()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var timeSlots = new List<TimeSlot>
        {
            CreateTimeSlot(clinicianId),
            CreateTimeSlot(clinicianId)
        };

        _timeSlotRepositoryMock
            .Setup(x => x.GetByClinicianIdAsync(clinicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeSlots);

        // Act
        var result = await _sut.GetByClinicianIdAsync(clinicianId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAvailableByClinicianIdAsync_ShouldReturnAvailableTimeSlots()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today);
        var timeSlots = new List<TimeSlot>
        {
            CreateTimeSlot(clinicianId, isAvailable: true),
            CreateTimeSlot(clinicianId, isAvailable: true)
        };

        _timeSlotRepositoryMock
            .Setup(x => x.GetAvailableByClinicianIdAsync(clinicianId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeSlots);

        // Act
        var result = await _sut.GetAvailableByClinicianIdAsync(clinicianId, date);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, slot => Assert.True(slot.IsAvailable));
    }

    private static TimeSlot CreateTimeSlot(Guid clinicianId, bool isAvailable = true)
    {
        return new TimeSlot
        {
            Id = Guid.NewGuid(),
            ClinicianId = clinicianId,
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow.AddMinutes(30),
            IsAvailable = isAvailable
        };
    }
}
