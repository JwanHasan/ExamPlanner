using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;
public class AssessmentHandInRepo : IAssessmentHandInRepo
{
    private readonly AppDbContext _context;
    public AssessmentHandInRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<AssessmentHandIn> AddHandInAsync(int assessmentEvenId, DateTime handInDateTime,
     int part, string note)
    {
        var added = new AssessmentHandIn
        {
            AssessmentEventId=assessmentEvenId,
            HandInDate=handInDateTime,
            Part=part,
            Note= note
        };
        await _context.AssessmentHandIn.AddAsync(added);
        await _context.SaveChangesAsync();
        return added;
    }

    public async Task<bool> DeleteHandInAsync(int id)
    {
        var deleted = await _context.AssessmentHandIn
        .Where(x=> x.Id==id).ExecuteDeleteAsync();
        return deleted>0;
    }

    public async Task<List<AssessmentHandIn>> GetAllEventAsync()
    {
        return await _context.AssessmentHandIn.ToListAsync();
    }

    public async Task<AssessmentEvent> GetAssessmentEventByIdAsync(int assessmentEvenId)
    {
        return await _context.AssessmentHandIn
        .Where(x=> x.AssessmentEventId==assessmentEvenId)
        .Select(x=> x.AssessmentEvent).FirstAsync();
    }

    public async Task<AssessmentHandIn?> GetHandInByIdAsync(int id)
    {
        return await _context.AssessmentHandIn.FindAsync(id);
    }

    

    public async Task<AssessmentHandIn?> UpdateHandInAsync(int id,int assessmentEvenId, DateTime handInDateTime, int part, string note)
    {
        var found = await GetHandInByIdAsync(id);
        if(found is null) return null;
        found.AssessmentEventId=assessmentEvenId;
        found.HandInDate=handInDateTime;
        found.Part=part;
        found.Note= note;
        await _context.SaveChangesAsync();
        return found;
    }
}