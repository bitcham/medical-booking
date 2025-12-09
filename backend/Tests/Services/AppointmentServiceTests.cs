using Core.Application.Dtos.Requests;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Application.Services.Impl;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentAssertions;
using Moq;
using backend.Tests.Fixtures;

namespace backend.Tests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<ITimeSlotRepository> _timeSlotRepositoryMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _timeSlotRepositoryMock = new Mock<ITimeSlotRepository>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new AppointmentService(
            _appointmentRepositoryMock.Object,
            _timeSlotRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WhenTimeSlotNotFound_ShouldThrowTimeSlotNotFoundException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var request = new CreateAppointmentRequest(Guid.NewGuid());

        _timeSlotRepositoryMock
            .Setup(x => x.GetByIdAsync(request.TimeSlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeSlot?)null);

        // Act
        var act = () => _sut.CreateAsync(patientId, request);

        // Assert
        await act.Should().ThrowAsync<TimeSlotNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenTimeSlotNotAvailable_ShouldThrowTimeSlotNotAvailableException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);
        var request = new CreateAppointmentRequest(timeSlot.Id);

        _timeSlotRepositoryMock
            .Setup(x => x.GetByIdAsync(request.TimeSlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeSlot);

        // Act
        var act = () => _sut.CreateAsync(patientId, request);

        // Assert
        await act.Should().ThrowAsync<TimeSlotNotAvailableException>();
    }

    [Fact]
    public async Task CreateAsync_WhenPatientNotFound_ShouldThrowPatientNotFoundException()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: true);
        var request = new CreateAppointmentRequest(timeSlot.Id);

        _timeSlotRepositoryMock
            .Setup(x => x.GetByIdAsync(request.TimeSlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeSlot);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = () => _sut.CreateAsync(patientId, request);

        // Assert
        await act.Should().ThrowAsync<PatientNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenValid_ShouldCreateAppointmentAndReserveTimeSlot()
    {
        // Arrange
        var patient = TestFixtures.Patients.Create();
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: true);
        var request = new CreateAppointmentRequest(timeSlot.Id, "Test notes");

        _timeSlotRepositoryMock
            .Setup(x => x.GetByIdAsync(request.TimeSlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeSlot);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var appointment = TestFixtures.Appointments.Create(patient, timeSlot);
                appointment.Id = id;
                appointment.Notes = request.Notes;
                return appointment;
            });

        // Act
        var result = await _sut.CreateAsync(patient.Id, request);

        // Assert
        result.PatientId.Should().Be(patient.Id);
        result.ClinicianId.Should().Be(timeSlot.ClinicianId);
        result.Status.Should().Be(AppointmentStatus.Pending);
        result.Notes.Should().Be("Test notes");
        timeSlot.IsAvailable.Should().BeFalse();
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetByIdAsync_WhenAppointmentNotFound_ShouldThrowAppointmentNotFoundException()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = () => _sut.GetByIdAsync(appointmentId);

        // Assert
        await act.Should().ThrowAsync<AppointmentNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnAppointment()
    {
        // Arrange
        var appointment = TestFixtures.Appointments.Create();

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.GetByIdAsync(appointment.Id);

        // Assert
        result.Id.Should().Be(appointment.Id);
    }

    #endregion

    #region GetByPatientId Tests

    [Fact]
    public async Task GetByPatientIdAsync_WhenNoAppointments_ShouldReturnEmptyList()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        _appointmentRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _sut.GetByPatientIdAsync(patientId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPatientIdAsync_WhenHasAppointments_ShouldReturnAll()
    {
        // Arrange
        var patient = TestFixtures.Patients.Create();
        var appointments = new List<Appointment>
        {
            TestFixtures.Appointments.Create(patient: patient),
            TestFixtures.Appointments.Create(patient: patient)
        };

        _appointmentRepositoryMock
            .Setup(x => x.GetByPatientIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        // Act
        var result = await _sut.GetByPatientIdAsync(patient.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public async Task CancelAsync_WhenAppointmentNotFound_ShouldThrowAppointmentNotFoundException()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var act = () => _sut.CancelAsync(appointmentId);

        // Assert
        await act.Should().ThrowAsync<AppointmentNotFoundException>();
    }

    [Fact]
    public async Task CancelAsync_WhenValid_ShouldCancelAppointmentAndReleaseTimeSlot()
    {
        // Arrange
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);
        var appointment = TestFixtures.Appointments.Create(timeSlot: timeSlot, status: AppointmentStatus.Pending);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.CancelAsync(appointment.Id);

        // Assert
        result.Status.Should().Be(AppointmentStatus.Cancelled);
        timeSlot.IsAvailable.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Confirm Tests

    [Fact]
    public async Task ConfirmAsync_WhenValid_ShouldConfirmAppointment()
    {
        // Arrange
        var appointment = TestFixtures.Appointments.Create(status: AppointmentStatus.Pending);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _sut.ConfirmAsync(appointment.Id);

        // Assert
        result.Status.Should().Be(AppointmentStatus.Confirmed);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Reschedule Tests

    [Fact]
    public async Task RescheduleAsync_WhenNewTimeSlotNotFound_ShouldThrowTimeSlotNotFoundException()
    {
        // Arrange
        var appointment = TestFixtures.Appointments.Create();
        var request = new RescheduleAppointmentRequest(Guid.NewGuid());

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _timeSlotRepositoryMock
            .Setup(x => x.GetByIdAsync(request.NewTimeSlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeSlot?)null);

        // Act
        var act = () => _sut.RescheduleAsync(appointment.Id, request);

        // Assert
        await act.Should().ThrowAsync<TimeSlotNotFoundException>();
    }

    [Fact]
    public async Task RescheduleAsync_WhenValid_ShouldRescheduleAppointment()
    {
        // Arrange
        var oldTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);
        var newTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: true);
        var appointment = TestFixtures.Appointments.Create(timeSlot: oldTimeSlot, status: AppointmentStatus.Pending);
        var request = new RescheduleAppointmentRequest(newTimeSlot.Id);

        _appointmentRepositoryMock
            .Setup(x => x.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _timeSlotRepositoryMock
            .Setup(x => x.GetByIdAsync(newTimeSlot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newTimeSlot);

        // Act
        var result = await _sut.RescheduleAsync(appointment.Id, request);

        // Assert
        result.TimeSlot.Id.Should().Be(newTimeSlot.Id);
        oldTimeSlot.IsAvailable.Should().BeTrue();
        newTimeSlot.IsAvailable.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
