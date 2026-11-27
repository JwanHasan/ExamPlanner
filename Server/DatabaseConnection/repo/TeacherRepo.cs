using System.Net.Security;
using System.Text.RegularExpressions;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace DatabaseConnection.repo;

public class TeacherRepo : ITeacherRepo
{
    private readonly AppDbContext _context;
    public TeacherRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<Teacher> AddAsync(int usernameId)
    {
        var crt= new Teacher
        {
            UserId=usernameId
        };
        await _context.Teacher.AddAsync(crt);
        await _context.SaveChangesAsync();
        return crt;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var isdeleted= await _context.Teacher.Where(x=> x.Id==id)
        .ExecuteDeleteAsync();
        return isdeleted>0;
    }

    public async Task<List<Teacher>> GetListAsync()
    {
        return await _context.Teacher.ToListAsync();
    }

    public async Task<Teacher?> GetTeacherAsync(int id)
    {
        return await _context.Teacher.FindAsync(id);
    }

    public async Task<User> GetUserAsync(int id)
    {
        return await _context.Teacher
        .Where(x=>x.Id==id).Select(x=> x.User).FirstAsync();
    }

    public async Task<Teacher?> UpdateAsync(int id, int usernameId)
    {
            var upt= await GetTeacherAsync(id);
            if(upt is null) return null;
            upt.UserId=usernameId;
            await  _context.SaveChangesAsync();
            return upt;
    }
}