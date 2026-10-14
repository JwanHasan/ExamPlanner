using System;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IRoomRepo
{
    Task<Room> AddAsync(int capacity,string name);

    Task<Room?> UpdateAsync(int id, int capacity,string name);

    Task<Room?> GetbyIdAsync(int id);

    Task<List<Room>> GetAllAsync();

    Task<bool> DeleteAsync(int id);
    Task SaveAsync();
}
