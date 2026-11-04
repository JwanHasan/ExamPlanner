namespace ExamPlannerServer.Dto;

public record PlanningEducationElementDto(
    Guid Id,
    string SourceChecksum,
    DateTime SourceModifiedAt,
    string Name,
    string Nickname,
    int Ects,
    int ExecutionElementCount,
    DateOnly StartDate,
    DateOnly EndDate,
    string ActivityOffering,
    string Prefix,
    string AssessmentType,
    bool CompletionAssessment,
    string GradingScale,
    bool CombinedWrittenOral,
    bool Oral,
    bool PracticalExam,
    bool Project,
    bool Written
);