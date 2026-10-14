using System;
using System.ComponentModel;
using DatabaseConnection.model;

namespace DatabaseConnection.repo;

public interface IPlanningEducationElementRepo
{
    Task<PlanningEducationElement> AddAsync ( string sourceChecksum, DateTime sourceModifiedAt,
        string name,string nickname,int ects,int executionElementCount,DateOnly startDate,
        DateOnly endDate,string activityOffering,string prefix,PlanningEducationElement.Assessment assessmentType,
        bool completionAssessment,string gradingScale,bool combinedWrittenOral,bool oral, bool practicalExam,
        bool project,bool written,AssessmentEvent assessmentEvent);
    Task<PlanningEducationElement?> GetByIdAsync(int id);
    Task<List<PlanningEducationElement>> GetAllAsync();
    Task<PlanningEducationElement?> UpdateByIdAsync(int id,
    string sourceChecksum,DateTime sourceModifiedAt,
    string name,string nickname,int ects,int executionElementCount,DateOnly startDate,
    DateOnly endDate,
    string activityOffering,
    string prefix,
    PlanningEducationElement.Assessment assessmentType,
    bool completionAssessment,
    string gradingScale,
    bool combinedWrittenOral,
    bool oral,
    bool practicalExam,
    bool project,
    bool written,
    AssessmentEvent assessmentEvent);

    Task<bool> DeleteAsyncById(int id);
    Task SaveAsync();
}