using System;
using System.Diagnostics.CodeAnalysis;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class ExamSessionRepo:IExamSessionRepo
{
    private readonly AppDbContext _context;
    public ExamSessionRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<ExamSession> AddAsyc(int assessmentEventId, int scheduleId, int roomId, DateOnly date, bool LockDate)
    {
        var n= new ExamSession{ AsseessmenEventId = assessmentEventId,
                                ScheduleId = scheduleId, RoomId = roomId,
                                 Date= date, LockDate = LockDate};
        await _context.AddAsync(n);
        await SaveChangesAsync();
        return n;
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var d= await _context.ExamSession.Where(x=> x.Id==id).ExecuteDeleteAsync();
        return d>0;
    }

    public async Task<List<ExamSession>> GetAllAsync()
    {
        return await _context.ExamSession.ToListAsync();
    }

    public async Task<List<ExamSession>> GetAllByAssessmentEventId(int id)
    {
        return await _context.ExamSession.Where(x=> x.AsseessmenEventId == id).ToListAsync();
    }

    public async Task<List<ExamSession>> GetAllByRoomId(int id)
    {
        return await _context.ExamSession.Where(x=> x.RoomId == id).ToListAsync();
    }

    public async Task<List<ExamSession>> GetAllByScheduleId(int id)
    {
        return await _context.ExamSession.Where(x=> x.ScheduleId == id).ToListAsync();
    }

    public async Task<ExamSession?> GetByIdAsync(int id)
    {
        return await _context.ExamSession.FindAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<ExamSession?> UpdateAsync(int id,int assessmentEventId, int scheduleId, int roomId, DateOnly date, bool LockDate)
    {
        var u = await GetByIdAsync(id);
            if(u is null) return null;
        u.AsseessmenEventId = assessmentEventId;
        u.ScheduleId = scheduleId;
        u.RoomId = roomId;
        u.Date= date;
        u.LockDate = LockDate;
        await SaveChangesAsync();
        return u;
    }
}
