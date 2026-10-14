namespace ExamPlannerServer.Import;

public class ParseResult
{
    public List<ParsedPlanningEducationElementRow> Rows { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}