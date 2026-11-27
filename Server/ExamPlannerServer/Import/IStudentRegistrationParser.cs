namespace ExamPlannerServer.Import;

public interface IStudentRegistrationParser
{
    StudentRegistrationParseResult Parse(Stream fileStream);
}

public class StudentRegistrationParseResult
{
    public List<ParsedStudentRegistrationRow> Rows { get; } = new();

    // Name of the sheet the data was read from; empty until a table was found.
    public string SheetName { get; set; } = "";

    // Row 0 is a problem with the whole file, otherwise the row number in the sheet.
    public List<ParseIssue> Issues { get; } = new();
}
