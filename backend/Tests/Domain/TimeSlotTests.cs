using Core.Domain.Entities;
using FluentAssertions;
using backend.Tests.Fixtures;

namespace backend.Tests.Domain;

public class TimeSlotTests
{
    [Fact]
    public void Reserve_WhenAvailable_ShouldSetIsAvailableToFalse()
    {
        // Arrange
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: true);

        // Act
        timeSlot.Reserve();

        // Assert
        timeSlot.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Reserve_WhenNotAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);

        // Act
        var act = () => timeSlot.Reserve();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public void Release_ShouldSetIsAvailableToTrue()
    {
        // Arrange
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: false);

        // Act
        timeSlot.Release();

        // Assert
        timeSlot.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Release_WhenAlreadyAvailable_ShouldRemainAvailable()
    {
        // Arrange
        var timeSlot = TestFixtures.TimeSlots.Create(isAvailable: true);

        // Act
        timeSlot.Release();

        // Assert
        timeSlot.IsAvailable.Should().BeTrue();
    }
}
