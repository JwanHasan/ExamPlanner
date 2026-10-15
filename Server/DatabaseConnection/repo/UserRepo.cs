using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class UserRepo : IUserRepo
{
    private readonly AppDbContext _context;
    public UserRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<User> AddUserAsync(string email, string name, string hashedPassword, Role role)
    {
        var added = new User
        {
            Email=email,
            Name=name,
            HashedPassword=hashedPassword,
            Role=role
        };
        
        await _context.User.AddAsync(added);
        await _context.SaveChangesAsync();
        return added;
    }

    public async Task<bool> DeleteUser(int id)
    {
        var deleted = await _context.User.Where(x=> x.Id==id).ExecuteDeleteAsync();
        return deleted>0;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.User.ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.User.FindAsync(id);
        
    }

    public async Task<User?> UpdateHashPasswordById(int id, string hashedPassword)
    {
        var changed = await GetUserByIdAsync(id);
        if(changed is null) return null;
        changed.HashedPassword=hashedPassword;
        await  _context.SaveChangesAsync();
        return changed;
    }

    public async Task<User?> UpdateNameById(int id, string name)
    {
        var changed = await GetUserByIdAsync(id);
        if(changed is null) return null;
        changed.Name=name;
        await _context.SaveChangesAsync();
        return changed;
    }

    public async Task<User?> UpdateUserById(int id, string email, string name, string hashedPassword, Role role)
    {
        var changed = await GetUserByIdAsync(id);
        if(changed is null) return null;
        changed.HashedPassword=hashedPassword;
        changed.Email=email;
        changed.Role=role;
        changed.Name= name;
        await _context.SaveChangesAsync();
        return changed;
    }

    public async Task<User?> UpdateUserEmailByIdAsync(int id, string email)
    {
        var changed = await GetUserByIdAsync(id);
        if(changed is null) return null;
        changed.Email=email;
        await _context.SaveChangesAsync();
        return changed;

    }

    public async Task<User?> UpdateUserRoleById(int id, Role role)
    {
        var changed = await GetUserByIdAsync(id);
        if(changed is null) return null;
        changed.Role=role;
        await _context.SaveChangesAsync();
        return changed;
    }
}