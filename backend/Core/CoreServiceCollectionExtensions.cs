using Core.Application.Dtos.Requests;
using Core.Application.Services;
using Core.Application.Services.Impl;
using Core.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class CoreServiceCollectionExtensions
{

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>();
        services.AddScoped<IValidator<RegisterPatientRequest>, RegisterPatientRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IClinicianService, ClinicianService>();
        services.AddScoped<IValidator<RegisterClinicianRequest>, RegisterClinicianRequestValidator>();
        
        return services;
    }
}