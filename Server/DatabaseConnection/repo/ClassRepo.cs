using DatabaseConnection.DBContext;
using DatabaseConnection.dto;
using DatabaseConnection.model;
using DatabaseConnection.repo;
using Microsoft.EntityFrameworkCore;

public class ClassRepo : IClassRepo
{
    private readonly AppDbContext _context;
    public ClassRepo(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Class> AddAsync(ClassDto classDto)
    {
        
        var newClass = new Class
        {
            CourseId = classDto.CourseId,
            ClassCode = classDto.ClassCode,
            NickName = classDto.NickName,
            Prefix= classDto.Prefix,
            Semester = classDto.Semester,
            StartDate = classDto.StartDate,
            EndDate = classDto.EndDate,
            CourseOffering = classDto.CourseOffering
        };
        await  _context.Class.AddAsync(newClass);
        await _context.SaveChangesAsync();
        return newClass;
    }

    public async Task<bool> DeleteClassAsync(int id)
    {
        var foundClass = await GetClassByIdAsync(id);
        if(foundClass is null)
            return false;
        _context.Class.Remove(foundClass);
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<List<Class>> GetAllClassesAsync()
    {
        return await _context.Class.ToListAsync();
    }

    public async Task<Class?> GetClassByClassCodeAsync(string classCode)
    {
        var foundClass = await _context.Class.FirstOrDefaultAsync(x=> x.ClassCode == classCode);
        return foundClass;
    }

    public async Task<Class?> GetClassByIdAsync(int id)
    {
        var foundClass= await _context.Class.FindAsync(id);
        return foundClass;
    }

    public async Task<Class?> UpdateClassAsync(int id, ClassDto classDto)
    {
        var updateClass = await GetClassByIdAsync(id);
        if(updateClass is null)
            return null;

        
            updateClass.CourseId = classDto.CourseId;
            updateClass.ClassCode = classDto.ClassCode;
            updateClass.NickName = classDto.NickName;
            updateClass.Prefix= classDto.Prefix;
            updateClass.Semester = classDto.Semester;
            updateClass.StartDate = classDto.StartDate;
            updateClass.EndDate = classDto.EndDate;
            updateClass.CourseOffering = classDto.CourseOffering;
        
        await _context.SaveChangesAsync();
        return updateClass;
    }
}