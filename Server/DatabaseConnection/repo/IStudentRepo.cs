using DatabaseConnection.model;
public interface IStudentRepo
{
    Task<Student?> GetByIdAsync(int id);

    Task<List<Student>> GetAllAsync();

    Task AddAsync(Student department);

    Task SaveChangesAsync();
}