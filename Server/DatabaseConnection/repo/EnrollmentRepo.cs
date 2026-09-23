using System.Runtime.CompilerServices;
using DatabaseConnection.DBContext;
using DatabaseConnection.dto;
using DatabaseConnection.model;
using DatabaseConnection.repo;
using Microsoft.EntityFrameworkCore;

public class EnrollmentRepo : IEnrollmentRepo
{
    private readonly AppDbContext _context;

    public EnrollmentRepo(AppDbContext context)
    {
        _context= context;
    }
    public async Task<Enrollment> AddAsync(EnrollmentDto enrollmentDto)
    {
        var enrol= new Enrollment{StudentId = enrollmentDto.StudentId, ClassId = enrollmentDto.ClassId};
        await _context.Enrollment.AddAsync(enrol);
        await _context.SaveChangesAsync();
        return enrol;

    }

    public async Task<bool> DeleteEnrollmentAsync(int studentId, int classId)
    {
        var findIt= await GetEnrollment(studentId,classId);
        if(findIt is null)
            return false;
        _context.Enrollment.Remove(findIt);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Enrollment>> GetAllEnrollmentAsync()
    {
        return await _context.Enrollment.ToListAsync();
    }

    public async Task<List<Class>> GetClassesByStudentId(int id)
    {
       var query =  _context.Enrollment.Where(e=> e.StudentId == id).Select(e=> e.Class);
       return await query.ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollment(int studentId, int classId)
    {
        var result = await _context.Enrollment.FindAsync(studentId,classId);
        return result;
    }

    public async Task<List<Student>> GetStudentsByClassId(int id)
    {
        var query =  _context.Enrollment.Where(e=> e.ClassId == id).Select(e=> e.Student);
       return await query.ToListAsync();
    }

    public async Task<Enrollment?> UpdateEnrollmentAsync(int studentId, int classId,EnrollmentDto enrollmentDto)
    {
        var update = await GetEnrollment(studentId,classId);
        if(update is null)
            return null;
        update.ClassId= enrollmentDto.ClassId;
        update.StudentId= enrollmentDto.StudentId;
        await _context.SaveChangesAsync();
        return update;
    }
}