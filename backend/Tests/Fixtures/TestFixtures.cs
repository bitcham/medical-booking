using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.ValueObjects;

namespace backend.Tests.Fixtures;

/// <summary>
/// Centralized test data factory to eliminate duplicate helper methods across tests.
/// </summary>
public static class TestFixtures
{
    public static class Users
    {
        public const string DefaultEmail = "test@example.com";
        public const string DefaultPassword = "hashed_password";
        public const string DefaultFirstName = "John";
        public const string DefaultLastName = "Doe";

        public static User Create(
            string email = DefaultEmail,
            string passwordHash = DefaultPassword,
            string firstName = DefaultFirstName,
            string lastName = DefaultLastName,
            UserRole role = UserRole.FrontDesk)
        {
            var user = User.Register(email, passwordHash, firstName, lastName);
            user.Role = role;
            return user;
        }
    }

    public static class Patients
    {
        public const string DefaultPhone = "123-456-7890";
        public static readonly DateOnly DefaultDateOfBirth = new(1990, 1, 1);

        public static Patient Create(
            User? user = null,
            string? phone = null,
            DateOnly? dateOfBirth = null,
            Address? address = null)
        {
            user ??= Users.Create(email: "patient@example.com", role: UserRole.Patient);
            
            return new Patient
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                PhoneNumber = phone ?? DefaultPhone,
                DateOfBirth = dateOfBirth ?? DefaultDateOfBirth,
                Address = address ?? new Address("123 Main St", "New York", "10001", "USA")
            };
        }
    }

    public static class Clinicians
    {
        public const string DefaultLicenseNumber = "LIC123456";
        public const string DefaultSpecialization = "General Dentist";
        public const string DefaultBio = "Experienced healthcare professional";

        public static Clinician Create(
            User? user = null,
            string? licenseNumber = null,
            string? specialization = null,
            string? bio = null)
        {
            user ??= Users.Create(email: "clinician@example.com", role: UserRole.Clinician);

            return new Clinician
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                LicenseNumber = licenseNumber ?? DefaultLicenseNumber,
                Specialization = specialization ?? DefaultSpecialization,
                Bio = bio ?? DefaultBio
            };
        }
    }

    public static class TimeSlots
    {
        public static readonly DateTimeOffset DefaultStartTime = new(2024, 1, 15, 9, 0, 0, TimeSpan.Zero);
        public static readonly DateTimeOffset DefaultEndTime = new(2024, 1, 15, 9, 30, 0, TimeSpan.Zero);

        public static TimeSlot Create(
            Clinician? clinician = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null,
            bool isAvailable = true)
        {
            clinician ??= Clinicians.Create();

            return new TimeSlot
            {
                Id = Guid.NewGuid(),
                ClinicianId = clinician.Id,
                Clinician = clinician,
                StartTime = startTime ?? DefaultStartTime,
                EndTime = endTime ?? DefaultEndTime,
                IsAvailable = isAvailable
            };
        }
    }

    public static class Appointments
    {
        public static Appointment Create(
            Patient? patient = null,
            TimeSlot? timeSlot = null,
            AppointmentStatus status = AppointmentStatus.Pending,
            string? notes = null)
        {
            patient ??= Patients.Create();
            timeSlot ??= TimeSlots.Create();

            return new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                Patient = patient,
                ClinicianId = timeSlot.ClinicianId,
                Clinician = timeSlot.Clinician,
                TimeSlotId = timeSlot.Id,
                TimeSlot = timeSlot,
                Status = status,
                Notes = notes
            };
        }
    }

    public static class RefreshTokens
    {
        public static RefreshToken Create(
            Guid? userId = null,
            string? token = null,
            DateTimeOffset? expires = null,
            DateTimeOffset? revoked = null)
        {
            return new RefreshToken
            {
                Token = token ?? Guid.NewGuid().ToString(),
                UserId = userId ?? Guid.NewGuid(),
                Expires = expires ?? DateTimeOffset.UtcNow.AddDays(7),
                Revoked = revoked
            };
        }

        public static RefreshToken CreateExpired(Guid? userId = null)
        {
            return Create(userId: userId, expires: DateTimeOffset.UtcNow.AddDays(-1));
        }

        public static RefreshToken CreateRevoked(Guid? userId = null)
        {
            return Create(userId: userId, revoked: DateTimeOffset.UtcNow);
        }
    }
}
