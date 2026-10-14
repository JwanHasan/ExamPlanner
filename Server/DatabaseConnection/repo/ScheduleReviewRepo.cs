using System;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class ScheduleReviewRepo:IScheduleReviewRepo
{
    private readonly AppDbContext _context;
    public ScheduleReviewRepo(AppDbContext context)
    {
        _context= context;
    }

    public async Task<ScheduleReview> AddAsync(int scheduleId, int teacherId, bool approved, bool completed, string remarks)
    {
        var u = new ScheduleReview
        {
            ScheduleId=scheduleId, TeacherId=teacherId,Approved=approved,Completed=completed,Remarks=remarks
        };
        await _context.ScheduleReview.AddAsync(u);
        await SaveAsync();
        return u;
    }

    public async Task<bool> DeleteAsync(int scheduleId, int teacherId)
    {
        var d= await _context.ScheduleReview
        .Where(x=> x.ScheduleId==scheduleId&&x.TeacherId==teacherId).ExecuteDeleteAsync();
        return d>0;
    }

    public async Task<List<ScheduleReview>> GetAllAsync()
    {
        return await _context.ScheduleReview.ToListAsync();
    }

    public async Task<List<ScheduleReview>> GetAllByScheduleIdAsync(int id)
    {
        return await _context.ScheduleReview.
        Where(x=>x.ScheduleId==id)
        .ToListAsync();
    }

    public async Task<List<ScheduleReview>> GetAllByTeacherIdAsync(int id)
    {
        return await _context.ScheduleReview.
        Where(x=>x.TeacherId==id)
        .ToListAsync();
    }

    public async Task<List<Schedule>> GetAllScheduleByTeacherIdAsync(int id)
    {
        return await _context.ScheduleReview.
        Where(x=>x.TeacherId==id).Select(x=> x.Schedule)
        .ToListAsync();
    }

    public async Task<List<Teacher>> GetAllTeacherByScheduleIdAsync(int id)
    {
        return await _context.ScheduleReview.
            Where(x=>x.ScheduleId==id).Select(x=> x.Teacher)
            .ToListAsync();
    }

    public async Task<ScheduleReview?> GetbyIdAsync(int scheduleId, int teacherId)
    {
        return await _context.ScheduleReview
            .FirstOrDefaultAsync(x=> x.ScheduleId ==scheduleId&& x.TeacherId==teacherId);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<ScheduleReview?> UpdateAsync(int scheduleId, int teacherId, bool approved, bool completed, string remarks)
    {
        var u = await GetbyIdAsync(scheduleId,teacherId);
            if(u is null)return null;
        u.ScheduleId=scheduleId;
        u.TeacherId=teacherId;
        u.Approved=approved;
        u.Completed=completed;
        u.Remarks=remarks;
        await SaveAsync();
        return u;
        }
}
