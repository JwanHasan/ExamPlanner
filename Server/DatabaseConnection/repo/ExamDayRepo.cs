using System;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class ExamDayRepo:IExamDayRepo
{
private readonly AppDbContext _context;
public ExamDayRepo(AppDbContext context)
{
    _context=context;
}

    public async Task<ExamDay> AddAsync(int scheduleId, DateTime date, ExamDayType examDayType, bool usable)
    {
        var add = new ExamDay
        {
            ScheduleId = scheduleId,
            Date = date,
            ExamDayType = examDayType,
            Usable= usable
        };
        await _context.ExamDay.AddAsync(add);
        await _context.SaveChangesAsync();
        return add;
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var del= await _context.ExamDay
            .Where(x=> x.Id == id).ExecuteDeleteAsync();
        return del>0;
    }

    public async Task<List<ExamDay>> GetAll()
    {
        return await _context.ExamDay.ToListAsync();
    }

    public async Task<List<ExamDay>> GetAllByScheduleId(int id)
    {
        return await _context.ExamDay
        .Where(x=> x.ScheduleId == id)
        .ToListAsync();
    }

    public async Task<ExamDay?> GetById(int id)
    {
        return await _context.ExamDay.FindAsync(id);
    }

    public async Task<DateTime?> GetDateAsyncByIdAsync(int id)
    {
         return await _context.ExamDay
        .Where(e => e.Id == id)
        .Select(e => (DateTime?)e.Date)
        .FirstOrDefaultAsync();
    }

    public async Task<ExamDayType?> GetExamDayTypeByIdAsync(int id)
    {
        var type= await GetById(id);
            if( type is null) return null;
        return type.ExamDayType;
    }

    public async Task<Schedule?> GetScheduleByIdAsync(int id)
    {
        return await _context.ExamDay.Where(x=>x.Id==id).Select(x=> x.Schedule)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> GetUsableByIdAsync(int id)
    {
        return await _context.ExamDay.Where(x=>x.Id==id).Select(x=> x.Usable)
            .FirstOrDefaultAsync();
    }

    public async Task<ExamDay?> UpdateByIdAsync(int id, int scheduleId, DateTime date, ExamDayType examDayType, bool usable)
    {
        var u = await GetById(id);
            if(u is null)  return null;
        u.ScheduleId = scheduleId;
        u.Date = date;
        u.ExamDayType = examDayType;
        u.Usable= usable;
        await _context.SaveChangesAsync();
        return u;
    }

    public async Task<ExamDay?> UpdateDateByIdAsync(int id, DateTime date)
    {
        var u = await GetById(id);
            if(u is null)  return null;
        u.Date = date;
        await _context.SaveChangesAsync();
        return u;
    }

    public async Task<ExamDay?> UpdateExamTypeByIdAsync(int id, ExamDayType type)
    {
        var u = await GetById(id);
            if(u is null)  return null;
        u.ExamDayType = type;
        await _context.SaveChangesAsync();
        return u;
    }

    public async Task<ExamDay?> UpdateScheduleIdByIdAsync(int id, int scheduleId)
    {
        var u = await GetById(id);
            if(u is null)  return null;
        u.ScheduleId = scheduleId;
        await _context.SaveChangesAsync();
        return u;
    }

    public async Task<ExamDay?> UpdateUsableByIdAsync(int id, bool usable)
    {
        var u = await GetById(id);
            if(u is null)  return null;
        u.Usable = usable;
        await _context.SaveChangesAsync();
        return u;
    }
}
