using DatabaseConnection.DBContext;
using ExamPlannerServer.Import;
using Microsoft.Extensions.Logging.Abstractions;

namespace TesProject;

// Builds the import service the way the API does (all importers, real parsers), for a given database context.
internal static class TestServices
{
    public static ImportService ImportService(AppDbContext context) => new(
        new PlanningElementImporter(new ExcelImportParser(), context, NullLogger<PlanningElementImporter>.Instance),
        new StudentRegistrationImporter(new StudentRegistrationParser(), context, NullLogger<StudentRegistrationImporter>.Instance));
}
