using System.Net;
using System.Net.Http.Json;
using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace backend.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for AppointmentController endpoints.
/// Tests the complete booking flow including time slot generation and appointment creation.
/// </summary>
public class AppointmentControllerTests : IntegrationTestBase
{
    public AppointmentControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region Create Appointment Tests

    [Fact]
    public async Task CreateAppointment_WithValidTimeSlot_ReturnsCreated()
    {
        // Arrange
        var (patientClient, _) = await CreateAuthenticatedPatientAsync();
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();

        // Generate time slots for clinician
        var generateRequest = new GenerateTimeSlotsRequest(DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var generateResponse = await clinicianClient.PostAsJsonAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots", 
            generateRequest);
        generateResponse.EnsureSuccessStatusCode();

        var timeSlots = await generateResponse.Content.ReadFromJsonAsync<IEnumerable<TimeSlotResponse>>();
        var firstSlot = timeSlots!.First();

        // Create appointment as patient
        var appointmentRequest = new CreateAppointmentRequest(firstSlot.Id, "Test appointment");

        // Act
        var response = await patientClient.PostAsJsonAsync("/api/v1/appointments", appointmentRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var appointment = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        appointment.Should().NotBeNull();
        appointment!.TimeSlot.Id.Should().Be(firstSlot.Id);
        appointment.Notes.Should().Be("Test appointment");
    }

    [Fact]
    public async Task CreateAppointment_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateAppointmentRequest(Guid.NewGuid());

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/appointments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAppointment_WithInvalidTimeSlot_ReturnsNotFound()
    {
        // Arrange
        var (patientClient, _) = await CreateAuthenticatedPatientAsync();
        var request = new CreateAppointmentRequest(Guid.NewGuid()); // Non-existent time slot

        // Act
        var response = await patientClient.PostAsJsonAsync("/api/v1/appointments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get Appointments Tests

    [Fact]
    public async Task GetAppointments_AsPatient_ReturnsPatientAppointments()
    {
        // Arrange
        var (patientClient, _) = await CreateAuthenticatedPatientAsync();

        // Act
        var response = await patientClient.GetAsync("/api/v1/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointments = await response.Content.ReadFromJsonAsync<IEnumerable<AppointmentResponse>>();
        appointments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAppointmentById_WhenExists_ReturnsAppointment()
    {
        // Arrange
        var (patientClient, _) = await CreateAuthenticatedPatientAsync();
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();

        // Create a time slot and appointment
        var generateRequest = new GenerateTimeSlotsRequest(DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var generateResponse = await clinicianClient.PostAsJsonAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots", 
            generateRequest);
        var timeSlots = await generateResponse.Content.ReadFromJsonAsync<IEnumerable<TimeSlotResponse>>();
        var firstSlot = timeSlots!.First();

        var createResponse = await patientClient.PostAsJsonAsync(
            "/api/v1/appointments", 
            new CreateAppointmentRequest(firstSlot.Id));
        var createdAppointment = await createResponse.Content.ReadFromJsonAsync<AppointmentResponse>();

        // Act
        var response = await patientClient.GetAsync($"/api/v1/appointments/{createdAppointment!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointment = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        appointment!.Id.Should().Be(createdAppointment.Id);
    }

    [Fact]
    public async Task GetAppointmentById_WhenNotExists_ReturnsNotFound()
    {
        // Arrange
        var (patientClient, _) = await CreateAuthenticatedPatientAsync();

        // Act
        var response = await patientClient.GetAsync($"/api/v1/appointments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Cancel Appointment Tests

    [Fact]
    public async Task CancelAppointment_WhenExists_ReturnsOkWithCancelledStatus()
    {
        // Arrange
        var (patientClient, _) = await CreateAuthenticatedPatientAsync();
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();

        // Create appointment
        var generateRequest = new GenerateTimeSlotsRequest(DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
        var generateResponse = await clinicianClient.PostAsJsonAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots", 
            generateRequest);
        var timeSlots = await generateResponse.Content.ReadFromJsonAsync<IEnumerable<TimeSlotResponse>>();
        var firstSlot = timeSlots!.First();

        var createResponse = await patientClient.PostAsJsonAsync(
            "/api/v1/appointments", 
            new CreateAppointmentRequest(firstSlot.Id));
        var createdAppointment = await createResponse.Content.ReadFromJsonAsync<AppointmentResponse>();

        // Act
        var response = await patientClient.PutAsync(
            $"/api/v1/appointments/{createdAppointment!.Id}/cancel", 
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancelledAppointment = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        cancelledAppointment!.Status.Should().Be(Core.Domain.Enums.AppointmentStatus.Cancelled);
    }

    #endregion

    #region Confirm Appointment Tests

    [Fact]
    public async Task ConfirmAppointment_WhenPending_ReturnsOkWithConfirmedStatus()
    {
        // Arrange
        var (patientClient, _) = await CreateAuthenticatedPatientAsync();
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();

        // Create appointment
        var generateRequest = new GenerateTimeSlotsRequest(DateOnly.FromDateTime(DateTime.Today.AddDays(4)));
        var generateResponse = await clinicianClient.PostAsJsonAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots", 
            generateRequest);
        var timeSlots = await generateResponse.Content.ReadFromJsonAsync<IEnumerable<TimeSlotResponse>>();
        var firstSlot = timeSlots!.First();

        var createResponse = await patientClient.PostAsJsonAsync(
            "/api/v1/appointments", 
            new CreateAppointmentRequest(firstSlot.Id));
        var createdAppointment = await createResponse.Content.ReadFromJsonAsync<AppointmentResponse>();

        // Act - Clinician confirms the appointment
        var response = await clinicianClient.PutAsync(
            $"/api/v1/appointments/{createdAppointment!.Id}/confirm", 
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var confirmedAppointment = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        confirmedAppointment!.Status.Should().Be(Core.Domain.Enums.AppointmentStatus.Confirmed);
    }

    #endregion

    #region Reschedule Appointment Tests

    [Fact]
    public async Task RescheduleAppointment_WithValidNewSlot_ReturnsOkWithNewTimeSlot()
    {
        // Arrange
        var (patientClient, _) = await CreateAuthenticatedPatientAsync();
        var (clinicianClient, clinician) = await CreateAuthenticatedClinicianAsync();

        // Generate time slots
        var generateRequest = new GenerateTimeSlotsRequest(DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
        var generateResponse = await clinicianClient.PostAsJsonAsync(
            $"/api/v1/clinicians/{clinician.Id}/timeslots", 
            generateRequest);
        var timeSlots = (await generateResponse.Content.ReadFromJsonAsync<IEnumerable<TimeSlotResponse>>())!.ToList();
        var firstSlot = timeSlots[0];
        var secondSlot = timeSlots[1];

        // Create appointment with first slot
        var createResponse = await patientClient.PostAsJsonAsync(
            "/api/v1/appointments", 
            new CreateAppointmentRequest(firstSlot.Id));
        var createdAppointment = await createResponse.Content.ReadFromJsonAsync<AppointmentResponse>();

        // Act - Reschedule to second slot
        var rescheduleRequest = new RescheduleAppointmentRequest(secondSlot.Id);
        var response = await patientClient.PutAsJsonAsync(
            $"/api/v1/appointments/{createdAppointment!.Id}/reschedule", 
            rescheduleRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var rescheduledAppointment = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        rescheduledAppointment!.TimeSlot.Id.Should().Be(secondSlot.Id);
    }

    #endregion
}
