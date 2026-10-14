using System;
using System.Runtime.Intrinsics.Arm;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class RoomRepo:IRoomRepo
{
    private readonly AppDbContext _context;
    public RoomRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<Room> AddAsync(int capacity, string name)
    {
        var a= new Room
        {
            Capacity=capacity, Name=name
        };
        await _context.Room.AddAsync(a);
        await SaveAsync();
        return a;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var d = await _context.Room.Where(x=> x.Id==id).ExecuteDeleteAsync();
        return d>0;
    }

    public async Task<List<Room>> GetAllAsync()
    {
        return await _context.Room.ToListAsync();
    }

    public async Task<Room?> GetbyIdAsync(int id)
    {
        return await _context.Room.FindAsync(id);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Room?> UpdateAsync(int id, int capacity, string name)
    {
        var u= await GetbyIdAsync(id);
            if(u is null) return null;
        u.Capacity= capacity;
        u.Name=name;
        await SaveAsync();
        return u;
    }
}
