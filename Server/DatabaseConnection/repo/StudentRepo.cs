using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using DatabaseConnection.DBContext;
using System.Net.Http.Headers;
using DatabaseConnection.dto;

public class StudentRepo : IStudentRepo
{
    private readonly AppDbContext _context;
    public StudentRepo(AppDbContext context)
    {
        _context= context;
    }
    public async Task<Student> AddAsync(StudentDto student)
    {
        var newStudent= new Student{ Name= student.Name, UserAccountId = student.UserAccountId, ViaId = student.ViaId};
        await _context.Student.AddAsync(newStudent);
        await SaveChangesAsync();
        return newStudent;
        
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

    public   async Task<List<Student>> GetAllAsync()
    {
        var list= await  _context.Student.ToListAsync();
        return list;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        var student = await _context.Student.FirstOrDefaultAsync(x=> x.Id==id);
        return student;
    }

    public async Task<Student?> GetByViaIdAsync(int viaId)
    {
        var student = await _context.Student.FirstOrDefaultAsync(x=> x.ViaId==viaId);
        return student;
    }

    public async Task SaveChangesAsync()
    {
        await  _context.SaveChangesAsync();
    }

    public async Task<Student?> UpdateByIdAsync(int id, StudentDto studentDto)
    {
        var student = await GetByIdAsync(id);
        if(student is null)
            return null;
        
        student.Name = studentDto.Name;
        student.ViaId= studentDto.ViaId;
        student.UserAccountId = studentDto.UserAccountId;
        await SaveChangesAsync();
        return student;

    }
}