using System.Net;
using System.Net.Http.Json;
using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using FluentAssertions;
using Xunit.Abstractions;

namespace backend.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for ClinicianController endpoints.
/// Tests clinician retrieval and time slot management.
/// </summary>
public class ClinicianControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper testOutputHelper) : IntegrationTestBase(factory)
{
    

    #region GetAll Tests

    [Fact]
    public async Task GetAllClinicians_WithAuth_ReturnsOk()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/clinicians");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var clinicians = await response.Content.ReadFromJsonAsync<IEnumerable<ClinicianResponse>>();
        clinicians.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllClinicians_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/clinicians");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetClinicianById_WhenExists_ReturnsOk()
    {
        // Arrange
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();

        // Act
        var response = await clinicianClient.GetAsync($"/api/v1/clinicians/{clinician.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ClinicianResponse>();
        result!.Id.Should().Be(clinician.Id);
        result.Specialization.Should().Be(clinician.Specialization);
    }

    [Fact]
    public async Task GetClinicianById_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync($"/api/v1/clinicians/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GetMe Tests

    [Fact]
    public async Task GetMe_AsClinician_ReturnsOwnProfile()
    {
        // Arrange
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();

        // Act
        var response = await clinicianClient.GetAsync("/api/v1/clinicians/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ClinicianResponse>();
        result!.Id.Should().Be(clinician.Id);
    }

    #endregion

    #region TimeSlot Generation Tests

    [Fact]
    public async Task GenerateTimeSlots_AsClinician_ReturnsCreatedWithSlots()
    {
        // Arrange
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();
        var request = new GenerateTimeSlotsRequest(DateOnly.FromDateTime(DateTime.Today.AddDays(10)));

        // Act
        var response = await clinicianClient.PostAsJsonAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var timeSlots = await response.Content.ReadFromJsonAsync<IEnumerable<TimeSlotResponse>>();
        timeSlots.Should().NotBeNull();
        timeSlots.Should().HaveCount(20); // 08:00-18:00, 30-min intervals = 20 slots
        timeSlots!.All(s => s.IsAvailable).Should().BeTrue();
    }

    [Fact]
    public async Task GetTimeSlots_ForClinician_ReturnsSlots()
    {
        // Arrange
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();

        // Generate slots first
        var generateRequest = new GenerateTimeSlotsRequest(DateOnly.FromDateTime(DateTime.Today.AddDays(11)));
        await clinicianClient.PostAsJsonAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots", 
            generateRequest);

        // Act
        var response = await clinicianClient.GetAsync($"/api/v1/clinicians/{clinician.Id}/timeslots");
        
        testOutputHelper.WriteLine(response.Content.ToString()); // Debug output
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var timeSlots = await response.Content.ReadFromJsonAsync<IEnumerable<TimeSlotResponse>>();
        timeSlots.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetTimeSlots_WithDateFilter_ReturnsOnlyAvailableSlotsForDate()
    {
        // Arrange
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();
        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(12));

        // Generate slots
        var generateRequest = new GenerateTimeSlotsRequest(targetDate);
        await clinicianClient.PostAsJsonAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots", 
            generateRequest);

        // Act
        var response = await clinicianClient.GetAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots?date={targetDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var timeSlots = await response.Content.ReadFromJsonAsync<IEnumerable<TimeSlotResponse>>();
        timeSlots.Should().NotBeNull();
        timeSlots!.All(s => s.IsAvailable).Should().BeTrue();
    }

    #endregion
}
