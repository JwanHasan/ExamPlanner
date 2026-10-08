namespace ExamPlannerServer.Import;

public interface IImportService
{
    Task<ImportResponse> ImportAsync(IFormFile file);
}