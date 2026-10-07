using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class CourseRepo : ICourseRepo
{
    private readonly AppDbContext _context;
    public CourseRepo(AppDbContext context)
    {
        _context= context;
    }
    public async Task<Course> AddCourseAsync(string SueCode, string Prefix, string name, int Semester, int ects, int PriorityTier)
    {
        var course= new Course{SueCode = SueCode, Prefix = Prefix, Name= name,
        Semester = Semester, ETCS= ects, PriorityTier = PriorityTier };
        await _context.Course.AddAsync( course);
        await SaveChangesAsync();
        return course;
        
    }

    public Task<bool> DeleteAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<Course>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        var course = await _context.Course.FindAsync(id);
        return course;
    }

    public async Task<Course?> GetCourseBySueCodeAsync(string SueCode)
    {
        var course = await _context.Course.FirstOrDefaultAsync(x=> x.SueCode == SueCode);
        return course;
    }

    public Task<Course> UpdateCourseAsync(string SueCode, string Prefix, string name, int Semester, int ects, int PriorityTier)
    {
        throw new NotImplementedException();
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}