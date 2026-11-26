namespace ExamPlannerServer.Import;

/// What ImportController calls: one entry point for every kind of file a planner can upload.
/// It hands the file to the importer that knows that kind of file.
public class ImportService(PlanningElementImporter planningElements) : IImportService
{
    public Task<ImportResponse> ImportAsync(IFormFile file) => planningElements.ImportAsync(file);
}
