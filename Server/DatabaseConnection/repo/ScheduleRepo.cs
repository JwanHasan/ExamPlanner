using System;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class ScheduleRepo: IScheduleRepo
{
    private readonly AppDbContext _context;

    public ScheduleRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Schedule> AddAsync(bool approve, string version)
    {
        var e = new Schedule{ Approved = approve, Version = version};
        await _context.Schedule.AddAsync(e);
        await SaveAsync();
        return e;

    }

    public async Task<bool> DeleteAsync(int id)
    {
        var d= await _context.Schedule.Where(x=> x.Id==id).ExecuteDeleteAsync();
        return d>0;
    }

    public async Task<List<Schedule>> GetAllAsync()
    {
        return await _context.Schedule.ToListAsync();
    }

    public async Task<Schedule?> GetbyIdAsync(int id)
    {
        return await _context.Schedule.FindAsync(id);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Schedule?> UpdateAsync(int id, bool approve, string version)
    {
        var schedule = await GetbyIdAsync(id);

        if (schedule == null)
        {
            return null;
        }

        schedule.Approved = approve;
        schedule.Version = version;

        await SaveAsync();

        return schedule;
    }
}
