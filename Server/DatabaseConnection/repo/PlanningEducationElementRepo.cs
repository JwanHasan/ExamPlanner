using System;
using System.Buffers.Binary;
using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace DatabaseConnection.repo;

public class PlanningEducationElementRepo:IPlanningEducationElementRepo
{
    private readonly AppDbContext _context;
    public PlanningEducationElementRepo(AppDbContext context)
    {
        _context=context;
    }

    public async Task<PlanningEducationElement> AddAsync(string sourceChecksum, DateTime sourceModifiedAt, string name, string nickname, int ects, int executionElementCount, DateOnly startDate, DateOnly endDate, string activityOffering, string prefix, PlanningEducationElement.Assessment assessmentType, bool completionAssessment, string gradingScale, bool combinedWrittenOral, bool oral, bool practicalExam, bool project, bool written, AssessmentEvent assessmentEvent)
    {
        var element = new PlanningEducationElement
    {
        SourceChecksum = sourceChecksum,
        SourceModifiedAt = sourceModifiedAt,
        Name = name,
        Nickmane = nickname,
        ECTS = ects,
        ExecutionElementCount = executionElementCount,
        StartDate = startDate,
        EndDate = endDate,
        ActivityOffering = activityOffering,
        Prefix = prefix,
        AssessmenType = assessmentType,
        CompletionAssesment = completionAssessment,
        GradingScale = gradingScale,
        CombinedWrittenOral = combinedWrittenOral,
        Oral = oral,
        PracticalExam = practicalExam,
        Project = project,
        Written = written,
        AssessmentEvent = assessmentEvent
    };
    await _context.PlanningEducationElement.AddAsync(element);
    await SaveAsync();
    return element;
    }

    public async Task<bool> DeleteAsyncById(int id)
    {
       var d= await _context.PlanningEducationElement
        .Where(x=> x.Id == id).ExecuteDeleteAsync();
        return d>0;
    }

    public async Task<List<PlanningEducationElement>> GetAllAsync()
    {
        return await _context.PlanningEducationElement.ToListAsync();
    }

    public async Task<PlanningEducationElement?> GetByIdAsync(int id)
    {
        return await _context.PlanningEducationElement.FindAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<PlanningEducationElement?> UpdateByIdAsync(int id, string sourceChecksum, DateTime sourceModifiedAt, string name, string nickname, int ects, int executionElementCount, DateOnly startDate, DateOnly endDate, string activityOffering, string prefix, PlanningEducationElement.Assessment assessmentType, bool completionAssessment, string gradingScale, bool combinedWrittenOral, bool oral, bool practicalExam, bool project, bool written, AssessmentEvent assessmentEvent)
    {
        var element = await GetByIdAsync(id);

    if (element == null)
    {
        throw new Exception(
            $"PlanningEducationElement with ID {id} was not found.");
    }

    element.SourceChecksum = sourceChecksum;
    element.SourceModifiedAt = sourceModifiedAt;
    element.Name = name;
    element.Nickmane = nickname;
    element.ECTS = ects;
    element.ExecutionElementCount = executionElementCount;
    element.StartDate = startDate;
    element.EndDate = endDate;
    element.ActivityOffering = activityOffering;
    element.Prefix = prefix;
    element.AssessmenType = assessmentType;
    element.CompletionAssesment = completionAssessment;
    element.GradingScale = gradingScale;
    element.CombinedWrittenOral = combinedWrittenOral;
    element.Oral = oral;
    element.PracticalExam = practicalExam;
    element.Project = project;
    element.Written = written;
    element.AssessmentEvent = assessmentEvent;

    await SaveAsync();
    return element;
    }
}
