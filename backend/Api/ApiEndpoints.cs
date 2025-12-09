using Asp.Versioning;

namespace backend;

[ApiVersion("1.0")]
public static class ApiEndpoints
{
    private const string BaseApi = $"/api/v{{version:apiVersion}}";

    public static class Auth
    {
        private const string Base = $"{BaseApi}/auth";
        
        public const string Register = $"{Base}/register";
        
        public const string RegisterPatient = $"{Base}/register/patient";
        
        public const string RegisterClinician = $"{Base}/register/clinician";
        
        public const string Login = $"{Base}/login";
        
        public const string RefreshToken = $"{Base}/refresh-token";
        
        public const string Logout = $"{Base}/logout";
    }
    
    public static class Users
    {
        private const string Base = $"{BaseApi}/users";
        
        public const string GetUserById = $"{Base}/{{id:guid}}";
    }
    
    public static class Patients
    {
        private const string Base = $"{BaseApi}/patients";
        
        public const string GetById = $"{Base}/{{id:guid}}";
        public const string GetMe = $"{Base}/me";
    }

    public static class Clinicians
    {
        private const string Base = $"{BaseApi}/clinicians";
        
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id:guid}}";
        public const string GetMe = $"{Base}/me";
        public const string GenerateTimeSlots = $"{Base}/{{id:guid}}/timeslots";
        public const string GetTimeSlots = $"{Base}/{{id:guid}}/timeslots";
    }

    public static class TimeSlots
    {
        private const string Base = $"{BaseApi}/timeslots";
        
        public const string Delete = $"{Base}/{{id:guid}}";
    }

    public static class Appointments
    {
        private const string Base = $"{BaseApi}/appointments";

        public const string Create = Base;
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id:guid}}";
        public const string Cancel = $"{Base}/{{id:guid}}/cancel";
        public const string Confirm = $"{Base}/{{id:guid}}/confirm";
        public const string Reschedule = $"{Base}/{{id:guid}}/reschedule";
    }
}