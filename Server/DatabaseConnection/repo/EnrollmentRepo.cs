using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class EnrollmentRepo : IEnrollmentRepo
{
    private readonly AppDbContext _context;
    public EnrollmentRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<Enrollment> AddEnrollmentAsync(int studentId, Guid PlanningElementId,string hold,int SourceImportId)
    {
        var created = new Enrollment
        {
            StudentId = studentId, PlanningElementId=PlanningElementId,
            Hold=hold, SourceImportId=SourceImportId
        };
        await _context.Enrollment.AddAsync(created);
        await _context.SaveChangesAsync();
        return created;
    }

    public async Task<bool> DeleteEnrollmentAsync(int studentId, Guid PlanningElementId)
    {
         var isDeleted = await _context.Enrollment
         .Where(x=> x.StudentId==studentId && x.PlanningElementId == PlanningElementId)
        .ExecuteDeleteAsync();
        return isDeleted>0;
    }

    public async Task<List<Enrollment>> GetAllAsync()
    {
        return await _context.Enrollment.ToListAsync();
    }

    public async Task<Enrollment?> GetEnrollmentsAsync(int studentId, Guid PlanningElementId)
    {
        var found= await _context.Enrollment.FirstOrDefaultAsync(x=> x.PlanningElementId==PlanningElementId
        && x.StudentId==studentId);
        if(found is null) return null;
        return found;
    }

    public async Task<List<Student>> GetStudentsByPlanningIdIdAsync(Guid PlanningElementId)
    {
        var result = await _context.Enrollment
        .Where(x=> x.PlanningElementId == PlanningElementId )
        .Select(x=> x.Student).ToListAsync();
        return result;
    }

        public async Task<Enrollment?> UpdateEnrollmentAsync(int currentStudentId,Guid oldPlanningElementId,int newStudentId,Guid newPlanningElementId,string hold,int SourceImportId)
       {
        
        var changed= await GetEnrollmentsAsync(currentStudentId,oldPlanningElementId);
        if(changed is null) return null;
        changed.StudentId= newStudentId;
        changed.PlanningElementId = newPlanningElementId;
        changed.Hold= hold;
        changed.SourceImportId = SourceImportId;
        await _context.SaveChangesAsync();
        return changed;
    }
}