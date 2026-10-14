using System;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IScheduleRepo
{
    Task<Schedule> AddAsync(bool approve, string version);

    Task<Schedule?> UpdateAsync(int id, bool approve, string version);

    Task<Schedule?> GetbyIdAsync(int id);

    Task<List<Schedule>> GetAllAsync();

    Task<bool> DeleteAsync(int id);
    Task SaveAsync();
}
