using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class ClassRepo : IClassRepo
{
    private readonly AppDbContext _context;
    public ClassRepo(AppDbContext context)
    {
        _context=context;
    }

    

    public  async Task<Class?> GetClassAsyncById(int id)
    {
        return await _context.Class.FindAsync(id);
    }

    public async Task<List<Class>> GetClassesAsync()
    {
        return await _context.Class.ToListAsync();
    }

    public async Task<List<Class>> GetClassesByCourseIdAsync(int courseId)
    {
        return await _context.Class.Where(p=> p.CourseId==courseId).ToListAsync();
    }

    public async Task<Class?> UpdateEndDateById(int id, DateTime endDate)
    {
        var update = await GetClassAsyncById(id);
        if(update is null) return null;
        update.EndDate = endDate;
        await _context.SaveChangesAsync();
        return update;
    }

    public async Task<Class?> UpdateNicknameById(int id, string nickname)
    {
        var update = await GetClassAsyncById(id);
        if(update is null) return null;
        update.NickName = nickname;
        await _context.SaveChangesAsync();
        return update;
    }

    public async Task<Class?> UpdateStartDateById(int id, DateTime startDate)
    {
        var update = await GetClassAsyncById(id);
        if(update is null) return null;
        update.StartDate = startDate;
        await _context.SaveChangesAsync();
        return update;
    }

    public async Task<Class?> UpdateClasscodeById(int id, string classCode)
    {
        var update = await GetClassAsyncById(id);
        if(update is null) return null;
        update.ClassCode = classCode;
        await _context.SaveChangesAsync();
        return update;
    }

    public async Task<Class> AddClassAsync(string classCode, int courseId, string nickname, DateTime startDate, DateTime endDate)
    {
        var product= new Class
        {
            ClassCode = classCode,
            CourseId = courseId,
            NickName = nickname,
            StartDate=startDate,
            EndDate=endDate
        };
        await _context.Class.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteClassAsyncById(int id)
    {
       var isDeleted =await _context.Class.Where(x=> x.Id ==id).ExecuteDeleteAsync();
       
       return isDeleted>0;

    }
}