using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;
public class AssessmentEventRepo:IAssessmentEventRepo
{
    private readonly AppDbContext _context;
    public AssessmentEventRepo(AppDbContext context)
    {
        _context= context;
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var deleted= await _context.AssessmentEvent
            .Where(x=> x.Id==id).ExecuteDeleteAsync();
        return deleted>0;
    }

    public async Task<List<AssessmentEvent>> GetAllAsync()
    {
        return await _context.AssessmentEvent.ToListAsync();
    }

  

    public async Task<AssessmentEvent?> GetByIdAsync(int id)
    {
        return await _context.AssessmentEvent.FindAsync(id);
    }

    public async Task<PlanningEducationElement?> GetEducationElementAsync(int id)
    {
        return await _context.AssessmentEvent
            .Where(x=> x.Id==id).Select(x=> x.PlanningEducationElement).FirstAsync();
    }

    public async Task<AssessmentEvent?> GetEventByPlanningElementIdAsync(Guid planningEducationElementId)
    {
        return await _context.AssessmentEvent
            .Where(x=> x.PlanningElementId==planningEducationElementId).FirstAsync();
    }

    public async Task<ExamType?> GetExamTypeByIdAsync(int id)
    {
       var type= await GetByIdAsync(id);
       if(type is null) return null;
       return type.ExamType;
    }

    public async  Task<string> GetNoteById(int id)
    {
        var type= await GetByIdAsync(id);
       if(type is null) return $"Assessment Event By Id:{id} does not exist";
       return type.Note;
    }

    public async Task<AssessmentEvent?> UpdateByIdAsync(int id,
      Guid planningEducationElementId, ExamType examType, string note, int RequiredExamDays)
    {
        var upt= await GetByIdAsync(id);
        if(upt is null) return null;
        upt.PlanningElementId= planningEducationElementId;
        upt.ExamType= examType;
        upt.Note= note;
        upt.RequiredExamDays=RequiredExamDays;
        await _context.SaveChangesAsync();
        return upt;
    }

    public async Task<AssessmentEvent?> UpdateExamTypeById(int id, ExamType type)
    {
        var upt= await GetByIdAsync(id);
        if(upt is null) return null;
        upt.ExamType= type;
        await _context.SaveChangesAsync();
        return upt;
    }

    public async Task<AssessmentEvent?> UpdateNoteById(int id, string note)
    {
        var upt= await GetByIdAsync(id);
        if(upt is null) return null;
        upt.Note= note;
        await _context.SaveChangesAsync();
        return upt;
    }
}