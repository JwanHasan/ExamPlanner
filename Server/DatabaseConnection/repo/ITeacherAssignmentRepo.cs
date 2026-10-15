using System;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface ITeacherAssignmentRepo
{
    Task<TeacherAssignment?> GetByIdAsync(int examsessionId, int teacherId);
    Task<List<TeacherAssignment>> GetAllAsync();
    Task<List<Teacher>> GetAllTeachersByExamSessionIdAsync( int id);
    Task<List<ExamSession>> GetExamSessionsByTeacherIdAsync(int id);
    Task<TeacherRole?> GetRoleByIdsAsync(int examsessionId, int teacherId);
    Task<TeacherAssignment?> UpdateAsync(int examsessionId, int teacherId, TeacherRole role);
    Task<bool> DeleteByIdsAsync(int examsessionId, int teacherId);
    Task<TeacherAssignment> AddAsync(int examsessionId, int teacherId, TeacherRole role);
    Task<TeacherAssignment?> UpdateRoleAsync(int examsessionId, int teacherId, TeacherRole role);



}
