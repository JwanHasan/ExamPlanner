using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class ExamSessionRepo : IExamSessionRepo
{
    private readonly AppDbContext _context;

    public ExamSessionRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ExamSession> AddAsync( int assessmentEventId, int scheduleId, int? roomId, int campusId, DateOnly date, bool LockDate, string groupLabel,
        TimeOnly? startTime, TimeOnly? endTime, string rowVersion
    )
    {
        var session = new ExamSession
        {
            AsseessmenEventId = assessmentEventId,
            ScheduleId = scheduleId,
            RoomId = roomId,
            CampusId = campusId,
            Date = date,
            LockDate = LockDate,
            GroupLabel = groupLabel,
            StartTime = startTime,
            EndTime = endTime,
            RowVersion = rowVersion
        };

        await _context.AddAsync(session);
        await SaveChangesAsync();
        return session;
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var deleted = await _context.ExamSession
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync();

        return deleted > 0;
    }

    public async Task<List<ExamSession>> GetAllAsync()
    {
        return await _context.ExamSession.ToListAsync();
    }

    public async Task<List<ExamSession>> GetAllByAssessmentEventId(int id)
    {
        return await _context.ExamSession
            .Where(x => x.AsseessmenEventId == id)
            .ToListAsync();
    }

    public async Task<List<ExamSession>> GetAllByRoomId(int id)
    {
        return await _context.ExamSession
            .Where(x => x.RoomId == id)
            .ToListAsync();
    }

    public async Task<List<ExamSession>> GetAllByScheduleId(int id)
    {
        return await _context.ExamSession
            .Where(x => x.ScheduleId == id)
            .ToListAsync();
    }

    public async Task<ExamSession?> GetByIdAsync(int id)
    {
        return await _context.ExamSession.FindAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<ExamSession?> UpdateAsync(
        int id, int assessmentEventId, int scheduleId, int? roomId, int campusId, DateOnly date, bool LockDate, string groupLabel,
        TimeOnly? startTime, TimeOnly? endTime, string rowVersion
    )
    {
        var session = await GetByIdAsync(id);

        if (session is null)
        {
            return null;
        }

        session.AsseessmenEventId = assessmentEventId;
        session.ScheduleId = scheduleId;
        session.RoomId = roomId;
        session.CampusId = campusId;
        session.Date = date;
        session.LockDate = LockDate;
        session.GroupLabel = groupLabel;
        session.StartTime = startTime;
        session.EndTime = endTime;
        session.RowVersion = rowVersion;

        await SaveChangesAsync();

        return session;
    }
}