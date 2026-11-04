using System;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class CourseTeacherRepo:ICourseTeacherRepo
{
    private readonly AppDbContext _context;
    public CourseTeacherRepo(AppDbContext context)
    {
        _context= context;
    }

    public async Task<CourseTeacher> AddAsync(int CourseId, int TeacherId)
    {
        var n = new CourseTeacher{CourseId= CourseId, TeacherId= TeacherId};
        await _context.CourseTeacher.AddAsync(n);
        return n;
    }

    public async Task<bool> DeleteAsync(int CourseId, int TeacherId)
    {
         var isDeleted =await _context.CourseTeacher.Where(x=> x.CourseId==CourseId && x.TeacherId == TeacherId)
        .ExecuteDeleteAsync();
        
        return isDeleted>0;
    }

    public async Task<List<CourseTeacher>> GetAllAsync()
    {
        return await _context.CourseTeacher.ToListAsync();
    }

    public async Task<List<CourseTeacher>?> GetbyCourseIdAsync(int id)
    {
        return await _context.CourseTeacher.Where(x=> x.CourseId==id).ToListAsync();
    }

    public async Task<List<CourseTeacher>?> GetbyTeacherIdAsync(int id)
    {
        return await _context.CourseTeacher.Where(x=> x.TeacherId==id).ToListAsync();
    }

    public async Task<CourseTeacher?> UpdateAsync(int CourseId, int TeacherId, int newCourseId,int newTeacherId)
    {
        
        var n = await _context.CourseTeacher.FirstOrDefaultAsync(x=> x.CourseId==CourseId && x.TeacherId==TeacherId );
            if(n is null) return null;
         new CourseTeacher{CourseId= CourseId, TeacherId= TeacherId};
        await _context.CourseTeacher.AddAsync(n);
        return n;
    }
}
