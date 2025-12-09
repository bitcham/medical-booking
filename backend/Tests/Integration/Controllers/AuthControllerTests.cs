using System.Net;
using System.Net.Http.Json;
using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace backend.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for AuthController endpoints.
/// Tests the complete HTTP pipeline including routing, authentication, and response handling.
/// </summary>
public class AuthControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new RegisterUserRequest(
            $"user_{Guid.NewGuid():N}@test.com",
            "Password123!",
            "John",
            "Doe"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().BeNullOrEmpty();
        authResponse.User.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid():N}@test.com";
        var request = new RegisterUserRequest(email, "Password123!", "John", "Doe");

        // Register first time
        await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Act - Register again with same email
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(409);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest("invalid-email", "Password123!", "John", "Doe");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange - First register a user
        var email = $"login_{Guid.NewGuid():N}@test.com";
        var password = "Password123!";
        var registerRequest = new RegisterUserRequest(email, password, "John", "Doe");
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest(email, password);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var email = $"wrongpass_{Guid.NewGuid():N}@test.com";
        var registerRequest = new RegisterUserRequest(email, "Password123!", "John", "Doe");
        await Client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginRequest(email, "WrongPassword!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@test.com", "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region RegisterPatient Tests

    [Fact]
    public async Task RegisterPatient_WithValidRequest_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new RegisterPatientRequest(
            $"patient_{Guid.NewGuid():N}@test.com",
            "Password123!",
            "John",
            "Doe",
            "123-456-7890",
            new DateOnly(1990, 1, 1),
            "123 Main St",
            "New York",
            "10001",
            "USA"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register/patient", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.User.Role.Should().Be("Patient");
    }

    #endregion

    #region RegisterClinician Tests

    [Fact]
    public async Task RegisterClinician_WithValidRequest_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new RegisterClinicianRequest(
            $"clinician_{Guid.NewGuid():N}@test.com",
            "Password123!",
            "Dr",
            "Smith",
            $"LIC{Guid.NewGuid():N}",
            "General Dentist",
            "Experienced dentist"
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register/clinician", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.User.Role.Should().Be("Clinician");
    }

    #endregion

    #region Protected Endpoint Tests

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/clinicians");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsOk()
    {
        // Arrange
        var authenticatedClient = await CreateAuthenticatedClientAsync();

        // Act
        var response = await authenticatedClient.GetAsync("/api/v1/clinicians");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
