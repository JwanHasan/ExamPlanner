namespace ExamPlannerServer.Import;

public record ImportIssue(string Severity, string Sheet, int Row, string? Column, string Message);

public class ImportCounts
{
    public int Courses { get; set; }
    public int Classes { get; set; }
    public int Students { get; set; }
    public int Enrollments { get; set; }
    public int Lecturers { get; set; }
    public int Exams { get; set; }
}

public class ImportResponse
{
    public string FileName { get; set; } = "";
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int SkippedRows { get; set; }
    public ImportCounts Created { get; set; } = new();
    public List<ImportIssue> Issues { get; set; } = new();
}

public class ImportFileException(string message) : Exception(message);