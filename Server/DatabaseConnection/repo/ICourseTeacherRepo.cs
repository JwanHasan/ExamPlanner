using System;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface ICourseTeacherRepo
{
    Task<List<CourseTeacher>> GetAllAsync();
    Task<List<CourseTeacher>?> GetbyCourseIdAsync(int id);
    Task<List<CourseTeacher>?> GetbyTeacherIdAsync(int id);
    Task<CourseTeacher> AddAsync(int CourseId, int TeacherId);
    Task<CourseTeacher?> UpdateAsync(int CourseId, int TeacherId, int newCourseId,int newTeacherId);
    Task<bool> DeleteAsync(int CourseId, int TeacherId);


}
