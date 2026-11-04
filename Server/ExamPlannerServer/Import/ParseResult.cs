namespace ExamPlannerServer.Import;

// One problem found. Row is the row number in the Excel sheet
// (the header is row 1), or 0 when the problem concerns the whole file.
public record ParseIssue(int Row, string Message);

public class ParseResult
{
    public List<ParsedPlanningEducationElementRow> Rows { get; set; } = new();

    // Name of the sheet the data was read from; empty until a table was found.
    public string SheetName { get; set; } = "";

    public List<ParseIssue> Issues { get; } = new();

    // The same problems as readable text ("Row 3: ..."; whole-file problems have no prefix).
    // Kept so existing callers (CourseService) keep working. It is coded from Issues,
    // so new problems must be added to Issues, not to this list.
    public List<string> Errors => Issues
        .Select(i => i.Row > 0 ? $"Row {i.Row}: {i.Message}" : i.Message)
        .ToList();
}
