using DatabaseConnection.model;

namespace DatabaseConnection.repo;
public interface IClassRepo
{
    Task<Class?> GetClassAsyncById(int id);
    Task<List<Class>> GetClassesAsync();
    Task<Class> UpdateClasscodeById(int id, string classCode);
    Task<List<Class>> GetClassesByCourseIdAsync(int courseId);
    
    Task<Class> UpdateNicknameById(int id,string nickname);
    Task<Class> UpdateStartDateById(int id, DateTime startDate);
    Task<Class> UpdateEndDateById(int id, DateTime endDate);
    Task<Class> AddClassAsync(string classCode, int courseId,string nickname,DateTime startDate,DateTime endDate);
    Task<bool> DeleteClassAsyncById(int id);

}