using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using DatabaseConnection.DBContext;
using System.Net.Http.Headers;

public class StudentRepo : IStudentRepo
{
    private readonly AppDbContext _context;
    public StudentRepo(AppDbContext context)
    {
        _context= context;
    }
    public async Task<Student> AddAsync(Student student)
    {
         await _context.Student.AddAsync(student);
         await SaveChangesAsync();
         return student;
         
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var student = await  GetByIdAsync(id);
        if (student!=null)
        {_context.Student.Remove(student);
            await  SaveChangesAsync();
            return true;
        }
        else return false;
    }

    public   async Task<IEnumerable<Student>> GetAllAsync()
    {
      var list= await  _context.Student.ToListAsync();
        return list;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        var student = await _context.Student.FirstOrDefaultAsync(x=> x.Id==id);
        return student;
    }

    public async Task SaveChangesAsync()
    {
       await  _context.SaveChangesAsync();
    }

    public async Task<Student> UpdateByIdAsync(int id, string name, int viaId)
    {
        var student = await GetByIdAsync(id);
        if(student!= null)
        {
            student.Name = name;
            student.ViaId=viaId;
             await SaveChangesAsync();
             return student;
        }
         else return null;
    }
}