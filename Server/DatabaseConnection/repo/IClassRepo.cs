using System.Security.Cryptography.X509Certificates;
using DatabaseConnection.dto;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface IClassRepo
{
    public Task<Class?> GetClassByClassCodeAsync(string classCode);
    public Task<Class> AddAsync(ClassDto classDto);
    public Task<List<Class>> GetAllClassesAsync();
    public Task<Class?> GetClassByIdAsync(int id);
    public Task<bool> DeleteClassAsync(int id);
    public Task<Class?> UpdateClassAsync(int id,ClassDto classDto);
}