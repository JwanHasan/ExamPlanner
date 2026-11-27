using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;
public class TeacherConstraintRepo : ITeacherConstraintRepo
{
    private readonly AppDbContext _context;
    public TeacherConstraintRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<TeacherConstraint> AddTeacherConstraintAsync(int teacherId, DateOnly constraintDate, ConstraintType constraintType, string note)
    {
        var constraint = new TeacherConstraint
        {
            TeacherId=teacherId,
            ConstraintDate=constraintDate,
            ConstraintType=constraintType,
            Note=note
        };
        await _context.TeacherConstraint.AddAsync(constraint);
        await _context.SaveChangesAsync();
        return constraint;
    }

    public async Task<bool> DeleteConstraintById(int id)
    {
        var deleted= await _context.TeacherConstraint.Where(x=> x.Id==id).ExecuteDeleteAsync();
        return deleted>0;
    }

    public async Task<List<TeacherConstraint>> GetAllConstraintsAsync()
    {
        return await _context.TeacherConstraint.ToListAsync();
    }

    public async Task<Teacher> GetTeacherByConstraintIdAsync(int id)
    {
        var found =await _context.TeacherConstraint.Where(x=> x.Id==id)
        .Select(x=> x.Teacher).FirstAsync();
        return found;
    }

    public async Task<TeacherConstraint?> GetTeacherConstraintByIdAsync(int id)
    {
        var constraint= await _context.TeacherConstraint.FindAsync(id);
        return constraint;
    }

    public async Task<TeacherConstraint?> UpdateConstraintTypeById(int id, ConstraintType constraintType)
    {
        var get= await GetTeacherConstraintByIdAsync(id);
        if(get is null) return null;
        get.ConstraintType = constraintType;
        await _context.SaveChangesAsync();
        return get;

    }

    public async Task<TeacherConstraint?> UpdateNoteById(int id, string note)
    {
        var get= await GetTeacherConstraintByIdAsync(id);
        if(get is null) return null;
        get.Note = note;
        await _context.SaveChangesAsync();
        return get;
    }

    public async Task<TeacherConstraint?> UpdateTeacherConstraintById(int id, int teacherId, DateOnly constraintDate, ConstraintType constraintType, string note)
    {
        var get = await GetTeacherConstraintByIdAsync(id);
        if(get == null) return null;
        get.TeacherId=teacherId;
        get.ConstraintDate=constraintDate;
        get.ConstraintType=constraintType;
        get.Note=note;
        await _context.SaveChangesAsync();
        return get;
    }
}