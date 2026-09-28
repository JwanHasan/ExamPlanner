using DatabaseConnection.dto;
using DatabaseConnection.model;
public interface IStudentRepo
{
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByViaIdAsync(int viaId) ;

    Task<List<Student>> GetAllAsync();

    Task<Student> AddAsync(StudentDto student);

    Task<Student?> UpdateByIdAsync(int id, StudentDto studentDto);

    Task<bool> DeleteByIdAsync(int id);



    Task SaveChangesAsync();
}