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
        
        public const string Login = $"{Base}/login";
        
        public const string RefreshToken = $"{Base}/refresh-token";
    }
    
    public static class Users
    {
        private const string Base = $"{BaseApi}/users";
        
        public const string GetUserById = $"{Base}/{{id:guid}}";
    }
    
}