namespace ExamPlannerServer.Import;

public class ParsedCourseRow
{
    public int SourceRowNumber { get; set; }
    public string? Ansvarlig { get; set; }
    public string? Prefix { get; set; }
    public string? Semester { get; set; }
    public string? SueCode { get; set; }
    public string? Name { get; set; }
    public string? Kaldenavn { get; set; }
    public string? Hold { get; set; }
    public string? Ects { get; set; }
    public string? City { get; set; }
}