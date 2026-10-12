using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;
public class StudentRepo : IStudentRepo
{
    private readonly AppDbContext _context;
     public StudentRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<Student> AddStudentAsync(int id, int userId)
    {
        var added= new Student
        {
            Id= id, UserId= userId
        };
        await _context.Student.AddAsync(added);
        await _context.SaveChangesAsync();
        return added;
    }

    public async Task<bool> DeleteStudentByIdAsync(int id)
    {
        var deleted = await _context.Student.Where(x=> x.Id==id).ExecuteDeleteAsync();
        return deleted>0;
    }

    public async Task<List<Enrollment>?> GetEnrollmentByStudentIdAsync(int id)
    {
        var list = await  _context.Student
        .Where(x=> x.Id==id).Select(x=> x.Enrollments)
        .FirstOrDefaultAsync();
        
        return list?.ToList();
            
        
    }

    public async Task<List<StudentExamAssignment>?> GetStudentExamAsssignmentByIdAsync(int id)
    {
        var collection = await _context.Student
        .Where(x=> x.Id==id).Select(x=> x.StudentExamAssignment)
        .FirstOrDefaultAsync();
        
        return collection?.ToList();

    }

    public async Task<User> GetUserByStudentIdAsync(int id)
    {
        var user = await  _context.Student.Where(x=> x.Id==id).Select(x=> x.User).FirstAsync();
        return user;
    }

    public async Task<Student?> UpdateStudentAsync(int id, int userId)
    {
        var find = await  _context.Student.FindAsync(id);
        if(find is null) return null;
        find.UserId = userId;
        await _context.SaveChangesAsync();
        return find;
    }
}