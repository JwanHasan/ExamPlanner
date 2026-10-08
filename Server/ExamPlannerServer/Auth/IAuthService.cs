namespace ExamPlannerServer.Auth;

public interface IAuthService
{
    Task<string?> LoginAsync(string email, string password);
}