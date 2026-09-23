namespace DatabaseConnection.repo;

using DatabaseConnection.DBContext;
using DatabaseConnection.dto;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

public class CourseRepo : ICourseRepo
{
    private readonly AppDbContext _context;
    public CourseRepo(AppDbContext context)
    {
        _context= context;
    }

    public async Task<Course> AddAsync(CourseDto courseDto)
    {
        
        var course = new Course
        { 
            SueCode= courseDto.SueCode,ETCS=courseDto.ETCS,Name=courseDto.Name
        };
        
        
        await _context.Course.AddAsync(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<bool> DeleteAsync(int id)
    {
       var course= await GetCourseAsyncById(id);
       if (course != null)
        {
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        } 
        else return false;

    }

    public async Task<List<Course>> GetAllCourseAsync()
    {
        return await _context.Course.ToListAsync();
    }

    public async Task<Course?> GetCourseAsyncById(int id)
    {
        var course= await _context.Course.FirstOrDefaultAsync(c=> c.Id == id);
        return course;
    }

    public async Task<Course?> UpdateCourseAsync(int courseId, CourseDto courseDto)
    {
        var item= await GetCourseAsyncById(courseId);
        if (item is null) 
            return null;

        item.Name= courseDto.Name;
        item.ETCS = courseDto.ETCS;
        item.SueCode = courseDto.SueCode;
        await _context.SaveChangesAsync();
        return item;
        
    }
    public async Task<Course?> GetCourseBySueCodeAsync(string sueCode)
    {
        var course = await _context.Course.FirstOrDefaultAsync(s=> s.SueCode==sueCode);
        return course;
    }

}