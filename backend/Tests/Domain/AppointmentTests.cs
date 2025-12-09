using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentAssertions;
using backend.Tests.Fixtures;

namespace backend.Tests.Domain;

public class AppointmentTests
{
    [Fact]
    public void Cancel_WhenPending_ShouldSetStatusToCancelledAndReleaseTimeSlot()
    {
        // Arrange
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);
        var appointment = TestFixtures.Appointments.Create(timeSlot: timeSlot, status: AppointmentStatus.Pending);

        // Act
        appointment.Cancel();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        timeSlot.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = TestFixtures.Appointments.Create(status: AppointmentStatus.Cancelled);

        // Act
        var act = () => appointment.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already cancelled*");
    }

    [Fact]
    public void Confirm_WhenPending_ShouldSetStatusToConfirmed()
    {
        // Arrange
        var appointment = TestFixtures.Appointments.Create(status: AppointmentStatus.Pending);

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Theory]
    [InlineData(AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Completed)]
    public void Confirm_WhenNotPending_ShouldThrowInvalidOperationException(AppointmentStatus status)
    {
        // Arrange
        var appointment = TestFixtures.Appointments.Create(status: status);

        // Act
        var act = () => appointment.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*pending*");
    }

    [Fact]
    public void Reschedule_WhenPending_ShouldReleaseOldAndReserveNewTimeSlot()
    {
        // Arrange
        var oldTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);
        var newTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: true);
        var appointment = TestFixtures.Appointments.Create(timeSlot: oldTimeSlot, status: AppointmentStatus.Pending);

        // Act
        appointment.Reschedule(newTimeSlot);

        // Assert
        oldTimeSlot.IsAvailable.Should().BeTrue();
        newTimeSlot.IsAvailable.Should().BeFalse();
        appointment.TimeSlotId.Should().Be(newTimeSlot.Id);
        appointment.TimeSlot.Should().BeSameAs(newTimeSlot);
    }

    [Theory]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Completed)]
    public void Reschedule_WhenInvalidStatus_ShouldThrowInvalidOperationException(AppointmentStatus status)
    {
        // Arrange
        var newTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: true);
        var appointment = TestFixtures.Appointments.Create(status: status);

        // Act
        var act = () => appointment.Reschedule(newTimeSlot);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cancelled or completed*");
    }

    [Fact]
    public void Reschedule_WhenNewTimeSlotNotAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var oldTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);
        var newTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);
        var appointment = TestFixtures.Appointments.Create(timeSlot: oldTimeSlot, status: AppointmentStatus.Pending);

        // Act
        var act = () => appointment.Reschedule(newTimeSlot);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending)]
    [InlineData(AppointmentStatus.Confirmed)]
    public void Reschedule_WhenValidStatus_ShouldSucceed(AppointmentStatus status)
    {
        // Arrange
        var oldTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);
        var newTimeSlot = TestFixtures.TimeSlots.Create(isAvailable: true);
        var appointment = TestFixtures.Appointments.Create(timeSlot: oldTimeSlot, status: status);

        // Act
        var act = () => appointment.Reschedule(newTimeSlot);

        // Assert
        act.Should().NotThrow();
    }
}
