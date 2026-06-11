namespace Application.DTOs
{
    public record RegisterRequest(string EmployeeCode, string Password, string FirstName, string LastName);
    public record LoginRequest(string EmployeeCode, string Password);
    public record TokenResponse(string AccessToken, string RefreshToken);
    public record RefreshRequest(string RefreshToken);
}
