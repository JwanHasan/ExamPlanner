using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface ITeacherRepo
{
    Task<Teacher> AddAsync(int usernameId);
    Task<Teacher?> GetTeacherAsync(int id);
    Task<User> GetUserAsync(int id);
    Task<List<Teacher>> GetListAsync();
    Task<Teacher?> UpdateAsync(int id,int usernameId);
    Task<bool> DeleteAsync(int id);
}