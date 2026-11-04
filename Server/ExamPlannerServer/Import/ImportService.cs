using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.Import;

/// Rows are matched on the id in column A:
///   new id -> inserted
///   known id, same checksum -> left alone
///   known id, new checksum  -> updated
/// Rows with problems are skipped and reported; the rest are still imported.
public class ImportService(IExcelImportParser parser, AppDbContext db, ILogger<ImportService> logger)
    : IImportService
{
    public async Task<ImportResponse> ImportAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var parsed = parser.Parse(stream);

        // ImportController 422 INVALID_IMPORT_DATA.
        var fileProblem = parsed.Issues.FirstOrDefault(i => i.Row == 0);
        if (fileProblem is not null)
            throw new ImportFileException(fileProblem.Message);

        // Rows the parser already rejected (one issue per row, with its row number).
        var issues = parsed.Issues
            .Select(i => new ImportIssue("error", parsed.SheetName, i.Row, null, i.Message))
            .ToList();
        var skipped = issues.Count;

        // Rows the parser accepted, but whose values do not fit the database.
        var valid = new List<(ParsedPlanningEducationElementRow Row, PlanningEducationElement.Assessment Assessment)>();
        foreach (var row in parsed.Rows)
        {
            var problem = FindDatabaseProblem(row, out var assessment);
            if (problem is null)
            {
                valid.Add((row, assessment));
            }
            else
            {
                issues.Add(new ImportIssue("error", parsed.SheetName, row.SourceRowNumber, problem.Value.Column, problem.Value.Message));
                skipped++;
            }
        }

        var (created, updated, unchanged) = await SaveAsync(valid);

        var imported = created + updated + unchanged;
        logger.LogInformation(
            "Import of {File}: {Created} created, {Updated} updated, {Unchanged} unchanged, {Skipped} skipped",
            file.FileName, created, updated, unchanged, skipped);

        return new ImportResponse
        {
            FileName = file.FileName,
            TotalRows = imported + skipped,
            ImportedRows = imported,
            SkippedRows = skipped,
            // A planning element is one offering of a
            // course (Kaldenavn, start/end date), so new elements are reported as "classes".
            // Updated and unchanged rows are only in ImportedRows.
            Created = new ImportCounts { Classes = created },
            Issues = issues
        };
    }

    // Checks what the database cannot store. Returns null when the row is fine.
    private static (string Column, string Message)? FindDatabaseProblem(
        ParsedPlanningEducationElementRow row, out PlanningEducationElement.Assessment assessment)
    {
        assessment = default;

        // The database stores ECTS as a whole number.
        if (row.Ects % 1 != 0)
            return ("ECTS", $"ECTS {row.Ects} has decimals, but the database stores whole numbers.");

        switch (row.AssessmentType.Trim().ToLowerInvariant())
        {
            case "intern":
                assessment = PlanningEducationElement.Assessment.Internal;
                return null;
            case "ekstern":
                assessment = PlanningEducationElement.Assessment.External;
                return null;
            default:
                return ("Bedømmelsestype", $"'{row.AssessmentType}' is not Intern or Ekstern.");
        }
    }

    private async Task<(int Created, int Updated, int Unchanged)> SaveAsync(
        List<(ParsedPlanningEducationElementRow Row, PlanningEducationElement.Assessment Assessment)> valid)
    {
        int created = 0, updated = 0, unchanged = 0;

        // One query for all ids in the file. AsNoTracking: we only compare, and updates are written
        // with ExecuteUpdate below, so EF's change tracker is not involved for existing rows.
        var ids = valid.Select(v => v.Row.PlanningElementId).ToList();
        var existing = await db.PlanningEducationElement
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        await using var transaction = await db.Database.BeginTransactionAsync();

        foreach (var (row, assessment) in valid)
        {
            var startDate = DateOnly.FromDateTime(row.StartDate);
            var endDate = DateOnly.FromDateTime(row.EndDate);
            var modifiedAt = AsUtc(row.SourceModifiedAt);

            if (!existing.TryGetValue(row.PlanningElementId, out var current))
            {
                db.PlanningEducationElement.Add(new PlanningEducationElement
                {
                    Id = row.PlanningElementId,
                    SourceChecksum = row.SourceChecksum,
                    SourceModifiedAt = modifiedAt,
                    Name = row.Name,
                    Nickmane = row.Nickname,
                    ECTS = (int)row.Ects,
                    ExecutionElementCount = row.ExecutionElementCount,
                    StartDate = startDate,
                    EndDate = endDate,
                    ActivityOffering = row.ActivityOffering,
                    // Prefix is not in the file; it is filled in by hand afterwards.
                    AssessmenType = assessment,
                    CompletionAssesment = row.CompletionAssessment,
                    GradingScale = row.GradingScale,
                    CombinedWrittenOral = row.CombinedWrittenOral,
                    Oral = row.Oral,
                    PracticalExam = row.PracticalExam,
                    Project = row.Project,
                    Written = row.Written,
                    // The entity defaults this to "new()", which would make EF also insert an empty
                    // AssessmentEvent (with no course, so the insert would fail). An element does not
                    // need an event yet, so it is left empty on purpose.
                    AssessmentEvent = null!
                });
                created++;
            }
            else if (current.SourceChecksum == row.SourceChecksum)
            {
                unchanged++; // the source system says nothing changed in this row
            }
            else
            {
                // Overwrites only what the file provides. Prefix (filled in by hand) is not touched.
                await db.PlanningEducationElement
                    .Where(e => e.Id == row.PlanningElementId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.SourceChecksum, row.SourceChecksum)
                        .SetProperty(e => e.SourceModifiedAt, modifiedAt)
                        .SetProperty(e => e.Name, row.Name)
                        .SetProperty(e => e.Nickmane, row.Nickname)
                        .SetProperty(e => e.ECTS, (int)row.Ects)
                        .SetProperty(e => e.ExecutionElementCount, row.ExecutionElementCount)
                        .SetProperty(e => e.StartDate, startDate)
                        .SetProperty(e => e.EndDate, endDate)
                        .SetProperty(e => e.ActivityOffering, row.ActivityOffering)
                        .SetProperty(e => e.AssessmenType, assessment)
                        .SetProperty(e => e.CompletionAssesment, row.CompletionAssessment)
                        .SetProperty(e => e.GradingScale, row.GradingScale)
                        .SetProperty(e => e.CombinedWrittenOral, row.CombinedWrittenOral)
                        .SetProperty(e => e.Oral, row.Oral)
                        .SetProperty(e => e.PracticalExam, row.PracticalExam)
                        .SetProperty(e => e.Project, row.Project)
                        .SetProperty(e => e.Written, row.Written));
                updated++;
            }
        }

        await db.SaveChangesAsync();   // writes the inserts
        await transaction.CommitAsync(); // inserts and updates become visible together

        return (created, updated, unchanged);
    }

    // The column in the database is "timestamp with time zone", and Npgsql refuses DateTime values
    // of unknown kind. Excel dates carry no time zone, so they are labelled UTC as they are.
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
