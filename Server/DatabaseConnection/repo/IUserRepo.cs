using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface IUserRepo
{
    Task<User> AddUserAsync(string email, string name,string hashedPassword, Role role);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> UpdateUserById(int id,string email, string name,string hashedPassword, Role role );
    Task<User?> UpdateUserRoleById(int id, Role role);
    Task<User?> UpdateUserEmailByIdAsync(int id, string email);
    Task<User?> UpdateHashPasswordById(int id, string hashedPassword);
    Task<User?> UpdateNameById(int id, string name);
    Task<bool> DeleteUser(int id);
}