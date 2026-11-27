namespace ExamPlannerServer.Import;

public interface IExcelImportParser
{
    ParseResult Parse(Stream fileStream);
}