using DatabaseConnection.model;
public interface IStudentRepo
{
    Task<Student?> GetByIdAsync(int id);

    Task<List<Student>> GetAllAsync();

    Task AddAsync(Student student);

    Task UpdtadecByIdAsync(int id);

    Task DeleteByIdAsync (int id);

    Task SaveChangesAsync();
}