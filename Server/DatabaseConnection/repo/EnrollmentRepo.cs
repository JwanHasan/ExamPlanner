using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class EnrollmentRepo : IEnrollmentRepo
{
    private readonly AppDbContext _context;
    public EnrollmentRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<Enrollment> AddEnrollmentAsync(int studentId, int courseId)
    {
        var created = new Enrollment
        {
            StudentId = studentId, CourseId=courseId
        };
        await _context.Enrollment.AddAsync(created);
        await _context.SaveChangesAsync();
        return created;
    }

    public async Task<bool> DeleteEnrollmentAsync(int studentId, int courseId)
    {
         var isDeleted = await _context.Enrollment
         .Where(x=> x.StudentId==studentId && x.CourseId == courseId)
        .ExecuteDeleteAsync();
        return isDeleted>0;
    }

    public async Task<List<Course>> GetCoursesByStudentIdAsync(int studentId)
    {
        var result = await _context.Enrollment
        .Where(x=> x.StudentId == studentId)
        .Select(x=> x.Course).ToListAsync();
        return result;
    }

    public async Task<List<Student>> GetStudentsByCourseIdAsync(int courseId)
    {
        var result = await _context.Enrollment
        .Where(x=> x.CourseId == courseId)
        .Select(x=> x.Student).ToListAsync();
        return result;
    }

    public async Task<List<Student>> GetStudentsBySuecodeAsync(string sueCode)
    {
        var result = await _context.Enrollment
        .Where(x=> x.Course.SueCode == sueCode)
        .Select(x=> x.Student).ToListAsync();
        return result;
    }

    public  async Task<Enrollment?> UpdateEnrollmentAsync(int currentStudentId, int currentCourseId, int newStudentId, int newCouseId)
    {
        
        var changed= await _context.Enrollment.FirstOrDefaultAsync(x=> x.CourseId==currentCourseId
        && x.StudentId==currentStudentId);
        if(changed is null) return null;
        changed.StudentId= newStudentId;
        changed.CourseId = newCouseId;
        await _context.SaveChangesAsync();
        return changed;
    }
}