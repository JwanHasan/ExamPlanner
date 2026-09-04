using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

public class StudentRepo : IStudentRepo
{
    private readonly AppDbContext _context;

    public StudentRepo(AppDbContext context)
    {
        _context= context;
    }
    public async Task AddAsync(Student student)
    {
         await _context.Student.AddAsync(student);
         await SaveChangesAsync();
         
    }

    public async Task DeleteByIdAsync(int id)
    {
        var student = await  GetByIdAsync(id);
        await _context.Remove
    }

    public async Task<List<Student>> GetAllAsync()
    {
       return await _context.Student.ToListAsync();
        
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        var result = await _context.Student.FirstOrDefaultAsync(x => x.Id == id);
        return result;
    }

    public async Task SaveChangesAsync()
    {
       await  _context.SaveChangesAsync();
    }

    public Task UpdtadecByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}