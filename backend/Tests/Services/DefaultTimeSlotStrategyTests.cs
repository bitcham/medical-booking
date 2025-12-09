using Core.Application.Services.Impl;

namespace backend.Tests.Services;

public class DefaultTimeSlotStrategyTests
{
    private readonly DefaultTimeSlotStrategy _sut;

    public DefaultTimeSlotStrategyTests()
    {
        _sut = new DefaultTimeSlotStrategy();
    }

    [Fact]
    public void GenerateSlots_ShouldGenerate20SlotsFor8To18Hours()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today);
        var offset = TimeSpan.Zero;

        // Act
        var slots = _sut.GenerateSlots(clinicianId, date, offset).ToList();

        // Assert
        // 08:00 to 18:00 = 10 hours, 30-min intervals = 20 slots
        Assert.Equal(20, slots.Count);
    }

    [Fact]
    public void GenerateSlots_FirstSlotShouldStartAt0800()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today);
        var offset = TimeSpan.Zero;

        // Act
        var slots = _sut.GenerateSlots(clinicianId, date, offset).ToList();

        // Assert
        Assert.Equal(8, slots.First().StartTime.Hour);
        Assert.Equal(0, slots.First().StartTime.Minute);
    }

    [Fact]
    public void GenerateSlots_LastSlotShouldEndAt1800()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today);
        var offset = TimeSpan.Zero;

        // Act
        var slots = _sut.GenerateSlots(clinicianId, date, offset).ToList();

        // Assert
        Assert.Equal(18, slots.Last().EndTime.Hour);
        Assert.Equal(0, slots.Last().EndTime.Minute);
    }

    [Fact]
    public void GenerateSlots_AllSlotsShouldBe30MinutesLong()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today);
        var offset = TimeSpan.Zero;

        // Act
        var slots = _sut.GenerateSlots(clinicianId, date, offset).ToList();

        // Assert
        Assert.All(slots, slot => 
            Assert.Equal(30, (slot.EndTime - slot.StartTime).TotalMinutes));
    }

    [Fact]
    public void GenerateSlots_AllSlotsShouldBeAvailable()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today);
        var offset = TimeSpan.Zero;

        // Act
        var slots = _sut.GenerateSlots(clinicianId, date, offset).ToList();

        // Assert
        Assert.All(slots, slot => Assert.True(slot.IsAvailable));
    }

    [Fact]
    public void GenerateSlots_AllSlotsShouldHaveCorrectClinicianId()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today);
        var offset = TimeSpan.Zero;

        // Act
        var slots = _sut.GenerateSlots(clinicianId, date, offset).ToList();

        // Assert
        Assert.All(slots, slot => Assert.Equal(clinicianId, slot.ClinicianId));
    }

    [Fact]
    public void GenerateSlots_ShouldApplyTimezoneOffset()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today);
        var offset = TimeSpan.FromHours(9); // KST

        // Act
        var slots = _sut.GenerateSlots(clinicianId, date, offset).ToList();

        // Assert
        Assert.All(slots, slot => Assert.Equal(offset, slot.StartTime.Offset));
    }
}
