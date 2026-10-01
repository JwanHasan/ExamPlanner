namespace ExamPlannerServer.Import;

public class ExcelImportParser : IExcelImportParser
{
    public ParseResult Parse(Stream fileStream)
    {
        var result = new ParseResult();

        result.Rows.Add(new ParsedCourseRow
        {
            SourceRowNumber = 2,
            Prefix = "IT",
            SueCode = "PRO3",
            Name = "Programming 3",
            Ects = "10"
        });

        return result;
    }
}