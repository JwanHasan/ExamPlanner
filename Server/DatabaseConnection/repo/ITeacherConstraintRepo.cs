using System.Dynamic;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface ITeacherConstraintRepo
{
    Task<Teacher> GetTeacherByConstraintIdAsync(int id);
    Task<TeacherConstraint?> GetTeacherConstraintByIdAsync(int id);
    Task<TeacherConstraint> AddTeacherConstraintAsync(int teacherId, DateOnly constraintDate,ConstraintType constraintType,string note);
    Task<TeacherConstraint?> UpdateTeacherConstraintById(int id,int teacherId,
     DateOnly constraintDate,ConstraintType constraintType,string note );
    Task<TeacherConstraint?> UpdateNoteById(int id, string note);
    Task<TeacherConstraint?> UpdateConstraintTypeById(int id, ConstraintType
     constraintType);
    Task<bool> DeleteConstraintById(int id);
    Task<List<TeacherConstraint>> GetAllConstraintsAsync();
}