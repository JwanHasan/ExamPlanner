using System;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public class CourseTeacherRepo:ICourseTeacherRepo
{
    private readonly AppDbContext _context;
    public CourseTeacherRepo(AppDbContext context)
    {
        _context= context;
    }

    public Task<CourseTeacher> AddAsync(int CourseId, int TeacherId)
    {
        throw new NotImplementedException();
    }

    public Task<CourseTeacher> DeleteAsync(int CourseId, int TeacherId)
    {
        throw new NotImplementedException();
    }

    public Task<List<CourseTeacher>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<CourseTeacher>?> GetbyCourseIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<CourseTeacher>?> GetbyTeacherIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<CourseTeacher> UpdateAsync(int CourseId, int TeacherId)
    {
        throw new NotImplementedException();
    }
}
