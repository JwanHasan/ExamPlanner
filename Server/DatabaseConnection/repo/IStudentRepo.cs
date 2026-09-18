using DatabaseConnection.model;
public interface IStudentRepo
{
    Task<Student?> GetByIdAsync(int id);

    Task<IEnumerable<Student>> GetAllAsync();

    Task<Student> AddAsync(Student student);

    Task<Student> UpdateByIdAsync(int id, string name, int viaId);

    Task<bool> DeleteByIdAsync (int id);

    Task SaveChangesAsync();
}