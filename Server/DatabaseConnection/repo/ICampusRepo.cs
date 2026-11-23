
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace DatabaseConnection.repo;

public interface ICampusRepo
{
    Task<Campus?> GetCampusByIdAsyc(int id);
    Task<List<Campus>> GetAllAsync();
    Task<Campus?> UpdateCampusByIdAsync(int id, string name, bool isVirtual);
    Task<bool> DeleteByIdAsync(int id);
}
