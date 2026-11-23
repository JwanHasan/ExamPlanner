

using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using DatabaseConnection.repo;
using Microsoft.EntityFrameworkCore;

public class CampusRepo:ICampusRepo
{
    private readonly AppDbContext _context;
    public CampusRepo(AppDbContext context)
    {
        _context= context;
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var d = await _context.Campuses.Where(x=> x.Id==id).ExecuteDeleteAsync();
        return d>0;
    }

    public async Task<List<Campus>> GetAllAsync()
    {
        return await _context.Campuses.ToListAsync();
    }

    public async Task<Campus?> GetCampusByIdAsyc(int id)
    {
       return await _context.Campuses.FindAsync(id);
    }

    public async Task<Campus?> UpdateCampusByIdAsync(int id, string name, bool isVirtual)
    {
        var u = await GetCampusByIdAsyc(id);
        if(u is null) return null;
        u.Name=name;
        u.IsVirtual=isVirtual;
         await _context.SaveChangesAsync();
        return u;
    }
}
