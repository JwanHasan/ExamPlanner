using System.Runtime.CompilerServices;
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
        Semester = Semester, ECTS= ects, PriorityTier = PriorityTier };
        await _context.Course.AddAsync( course);
        await SaveChangesAsync();
        return course;
        
    }

    public async Task<bool> DeleteAsync(int id )
    {
        var found =await  GetCourseByIdAsync(id);
        if(found is null) return false;

        _context.Course.Remove(found);
        return true;

    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await _context.Course.ToListAsync();
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

    public async Task<Course> UpdateCourseBySuecodeAsync(string SueCode, string Prefix, string name,
     int Semester, int ects, int PriorityTier)
    {
        var found = await GetCourseBySueCodeAsync(SueCode);
        if(found is null) return new Course{};
        found.SueCode = SueCode;
        found.Prefix = Prefix;
        found.Name=name;
        found.Semester = Semester;
        found.ECTS =ects;
        found.PriorityTier = PriorityTier;
        await SaveChangesAsync();
        return found;


    }
    public async Task<Course> UpdateCourseByIdAsync(int id,string SueCode, string Prefix, string name, int Semester, int ects, int PriorityTier)
    {
        var found = await GetCourseByIdAsync(id);
        if(found is null) return new Course{};
        found.SueCode = SueCode;
        found.Prefix = Prefix;
        found.Name=name;
        found.Semester = Semester;
        found.ECTS =ects;
        found.PriorityTier = PriorityTier;
        await SaveChangesAsync();
        return found;
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}