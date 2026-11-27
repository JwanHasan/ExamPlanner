namespace ExamPlannerServer.Import;

/// What ImportController calls: one entry point for every kind of file a planner can upload.
/// It looks at the columns to see which kind of file it is, and hands it to the importer that knows that kind:
///   the Opslagsark (it has a student column)      -> StudentRegistrationImporter
///   anything else (the planning-element export)   -> PlanningElementImporter
public class ImportService(
    PlanningElementImporter planningElements, StudentRegistrationImporter studentRegistrations) : IImportService
{
    public async Task<ImportResponse> ImportAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var isOpslagsark = ImportFileDetector.IsStudentRegistrations(stream);

        return isOpslagsark
            ? await studentRegistrations.ImportAsync(file)
            : await planningElements.ImportAsync(file);
    }
}
