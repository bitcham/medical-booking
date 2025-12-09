using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Dtos.Requests;
using Core.Application.Dtos.Responses;
using Infra.Data;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests.Integration;

/// <summary>
/// Base class for integration tests providing common functionality
/// such as authenticated HTTP client creation and database seeding.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Creates an authenticated HTTP client by registering a user and setting the JWT token.
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync(
        string email = "test@test.com",
        string password = "Password123!",
        string firstName = "Test",
        string lastName = "User")
    {
        var client = Factory.CreateClient();

        // Register user
        var registerRequest = new RegisterUserRequest(email, password, firstName, lastName);
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            // User might already exist, try login
            var loginRequest = new LoginRequest(email, password);
            response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        return client;
    }

    /// <summary>
    /// Creates an authenticated patient client.
    /// </summary>
    protected async Task<(HttpClient Client, PatientResponse Patient)> CreateAuthenticatedPatientAsync()
    {
        var client = Factory.CreateClient();

        var registerRequest = new RegisterPatientRequest(
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

        var response = await client.PostAsJsonAsync("/api/v1/auth/register/patient", registerRequest);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Get patient info
        var patientResponse = await client.GetFromJsonAsync<PatientResponse>("/api/v1/patients/me");

        return (client, patientResponse!);
    }

    /// <summary>
    /// Creates an authenticated clinician client.
    /// </summary>
    protected async Task<(HttpClient Client, ClinicianResponse Clinician)> CreateAuthenticatedClinicianAsync()
    {
        var client = Factory.CreateClient();

        var registerRequest = new RegisterClinicianRequest(
            $"clinician_{Guid.NewGuid():N}@test.com",
            "Password123!",
            "Dr",
            "Smith",
            $"LIC{Guid.NewGuid():N}",
            "General Dentist",
            "Experienced dentist with 10 years of practice."
        );

        var response = await client.PostAsJsonAsync("/api/v1/auth/register/clinician", registerRequest);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Get clinician info
        var clinicianResponse = await client.GetFromJsonAsync<ClinicianResponse>("/api/v1/clinicians/me");

        return (client, clinicianResponse!);
    }

    /// <summary>
    /// Gets the database context for direct database manipulation in tests.
    /// </summary>
    protected AppDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
