using System.Security.Cryptography.X509Certificates;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class StudentExamAssignmentRepo : IStudentExamAssignmentRepo
{
    private readonly AppDbContext _context;

    public StudentExamAssignmentRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<StudentExamAssignment> AddAsync(int studentId, int examSessionId, bool extraTime)
    {
        var item = new StudentExamAssignment
        {
            StudentId=studentId,
            ExamSessionId=examSessionId,
            ExtraTime=extraTime
        };
        await _context.StudentExamAssignment.AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(int studentId, int examSessionId)
    {
        var deleted = await _context.StudentExamAssignment
            .Where(x=> x.StudentId==studentId && x.ExamSessionId==examSessionId)
            .ExecuteDeleteAsync();
        return deleted>0;

    }

    public async Task<List<StudentExamAssignment>> GetAllAsync()
    {
        return await _context.StudentExamAssignment.ToListAsync();
    }

    public async Task<List<ExamSession>> GetAllExamSessionByStudentId(int studentId)
    {
        return await _context.StudentExamAssignment
        .Where(x=> x.StudentId == studentId).Select(x=> x.ExamSession).ToListAsync();
    }

    public async Task<List<Student>> GetAllStudentByExamSessionId(int examSessionId)
    {
        return await _context.StudentExamAssignment
        .Where(x=> x.ExamSessionId == examSessionId).Select(x=> x.Student).ToListAsync();
    }

    public async Task<StudentExamAssignment?> GetByIdAsync(int studentId, int examSessionId)
    {
        var found = await _context.StudentExamAssignment
            .Where(x=> x.StudentId==studentId && x.ExamSessionId==examSessionId)
            .FirstAsync();
        return found;
    }

    public async Task<StudentExamAssignment?> UpdateAsync(int studentId, int examSessionId, bool extraTime)
    {
        var update = await GetByIdAsync(studentId,examSessionId);
        if(update is null) return null;
        update.StudentId=studentId;
        update.ExamSessionId=examSessionId;
        update.ExtraTime= extraTime;
        await _context.SaveChangesAsync();
        return update;
    }

    public async Task<StudentExamAssignment?> UpdateExtratimeAsync(int studentId, int examSessionId, bool extraTime)
    {
        var update = await GetByIdAsync(studentId,examSessionId);
        if(update is null) return null;
        update.ExtraTime= extraTime;
        await _context.SaveChangesAsync();
        return update;
    }
}