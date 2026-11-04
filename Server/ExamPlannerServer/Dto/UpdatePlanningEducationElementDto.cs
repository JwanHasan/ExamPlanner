namespace ExamPlannerServer.Dto;

public class UpdatePlanningEducationElementDto {
    public string Name { get; set; } = "";
    public string Nickname { get; set; } = "";
    public int Ects { get; set; }
    public int ExecutionElementCount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string ActivityOffering { get; set; } = "";
    public string Prefix { get; set; } = "";
    public string AssessmentType { get; set; } = "";
    public bool CompletionAssessment { get; set; }
    public string GradingScale { get; set; } = "";
    public bool CombinedWrittenOral { get; set; }
    public bool Oral { get; set; }
    public bool PracticalExam { get; set; }
    public bool Project { get; set; }
    public bool Written { get; set; }
}