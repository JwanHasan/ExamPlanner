using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.Import;

/// Every valid row is saved as a PlanningEducationElement, and also gets a Course and a Class:
///   Class  = this offering of the course (ClassCode = Kaldenavn), created or updated per row
///   Course = the subject (programme prefix + code, e.g. IT + ADS1), created once and then left alone
///            so that what planners edit by hand is never overwritten by a re-import
///
/// Planning elements are matched on the id in column A:
///   new id -> inserted
///   known id, same checksum -> left alone
///   known id, new checksum  -> updated
/// Rows with problems are skipped and reported; the rest are still imported.
public class ImportService(IExcelImportParser parser, AppDbContext db, ILogger<ImportService> logger)
    : IImportService
{
    private sealed record ValidRow(
        ParsedPlanningEducationElementRow Row,
        PlanningEducationElement.Assessment Assessment,
        string Prefix,
        string CourseCode);

    private sealed record SaveResult(
        int Created, int Updated, int Unchanged, int CoursesCreated, int ClassesCreated);

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
        var valid = new List<ValidRow>();
        foreach (var row in parsed.Rows)
        {
            var problem = FindDatabaseProblem(row, out var assessment, out var prefix, out var courseCode);
            if (problem is not null)
            {
                issues.Add(new ImportIssue("error", parsed.SheetName, row.SourceRowNumber, problem.Value.Column, problem.Value.Message));
                skipped++;
                continue;
            }

            valid.Add(new ValidRow(row, assessment, prefix!, courseCode!));

            // Not a reason to skip the row: the course is saved, only without a campus.
            if (CourseDerivation.Campus(row.ActivityOffering).Length == 0)
                issues.Add(new ImportIssue("warning", parsed.SheetName, row.SourceRowNumber, "Aktivitetsudbud",
                    "No campus found in the text, so the course is saved without a campus."));
        }

        var saved = await SaveAsync(valid);

        var imported = saved.Created + saved.Updated + saved.Unchanged;
        logger.LogInformation(
            "Import of {File}: {Created} elements created, {Updated} updated, {Unchanged} unchanged, " +
            "{Courses} courses and {Classes} classes created, {Skipped} rows skipped",
            file.FileName, saved.Created, saved.Updated, saved.Unchanged, saved.CoursesCreated, saved.ClassesCreated, skipped);

        return new ImportResponse
        {
            FileName = file.FileName,
            TotalRows = imported + skipped,
            ImportedRows = imported,
            SkippedRows = skipped,
            // What the website's result dialog shows: the courses and classes that are new in the database.
            // Rows that only updated or confirmed existing data count in ImportedRows, not here.
            Created = new ImportCounts { Courses = saved.CoursesCreated, Classes = saved.ClassesCreated },
            Issues = issues
        };
    }

    // Checks what the database cannot store, and works out the course values. Null when the row is fine.
    private static (string Column, string Message)? FindDatabaseProblem(
        ParsedPlanningEducationElementRow row, out PlanningEducationElement.Assessment assessment,
        out string? prefix, out string? courseCode)
    {
        assessment = default;
        prefix = CourseDerivation.Prefix(row.Nickname);
        courseCode = CourseDerivation.CourseCode(row.Name);

        // The database stores ECTS as a whole number.
        if (row.Ects % 1 != 0)
            return ("ECTS", $"ECTS {row.Ects} has decimals, but the database stores whole numbers.");

        if (prefix is null)
            return ("Kaldenavn", $"'{row.Nickname}' does not start with a two-letter programme prefix.");

        if (courseCode is null)
            return ("Navn", $"No course code found at the start of '{row.Name}'.");

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

    private async Task<SaveResult> SaveAsync(List<ValidRow> valid)
    {
        int created = 0, updated = 0, unchanged = 0, coursesCreated = 0, classesCreated = 0;

        // One query for all ids in the file. AsNoTracking: we only compare, and updates are written
        // with ExecuteUpdate below, so EF's change tracker is not involved for existing rows.
        var ids = valid.Select(v => v.Row.PlanningElementId).ToList();
        var existing = await db.PlanningEducationElement
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        // Classes and courses are tracked (they are changed through the normal save below).
        // A class is found by its code; that is how a row finds its course again after a planner
        // has edited the course by hand. Only a row with a new class looks for the course by prefix + code.
        var classCodes = valid.Select(v => v.Row.Nickname).Distinct().ToList();
        var classes = (await db.Class.Where(c => classCodes.Contains(c.ClassCode)).ToListAsync())
            .GroupBy(c => c.ClassCode)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Id).First());

        var prefixes = valid.Select(v => v.Prefix).Distinct().ToList();
        var codes = valid.Select(v => v.CourseCode).Distinct().ToList();
        var courses = (await db.Course.Where(c => prefixes.Contains(c.Prefix) && codes.Contains(c.SueCode)).ToListAsync())
            .GroupBy(c => CourseDerivation.CourseKey(c.Prefix, c.SueCode))
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Id).First());

        await using var transaction = await db.Database.BeginTransactionAsync();

        foreach (var item in valid)
        {
            var (row, assessment, prefix, courseCode) = item;
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
                    Written = row.Written
                    // AssessmentEvent is left empty on purpose: an element does not need an event yet.
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

            // Course and class are looked at for EVERY valid row, not only for new or changed elements.
            // That way a database that was filled by an earlier import (elements only) gets its
            // courses and classes the next time the same file is imported.
            var classStart = AsUtc(row.StartDate);
            var classEnd = AsUtc(row.EndDate);

            if (classes.TryGetValue(row.Nickname, out var existingClass))
            {
                // The dates and nickname come from the file, so the file wins. The course is not touched.
                existingClass.NickName = row.Nickname;
                existingClass.StartDate = classStart;
                existingClass.EndDate = classEnd;
                continue;
            }

            var key = CourseDerivation.CourseKey(prefix, courseCode);
            if (!courses.TryGetValue(key, out var course))
            {
                course = new Course
                {
                    SueCode = courseCode,
                    Prefix = prefix,
                    Name = CourseDerivation.CourseName(row.Name),
                    ECTS = (int)row.Ects,
                    Campus = CourseDerivation.Campus(row.ActivityOffering)
                    // Semester and PriorityTier stay 0: they are filled in by hand afterwards.
                };
                db.Course.Add(course);
                courses[key] = course; // later rows of this file with the same course reuse it
                coursesCreated++;
            }

            var newClass = new Class
            {
                ClassCode = row.Nickname,
                NickName = row.Nickname,
                StartDate = classStart,
                EndDate = classEnd,
                Course = course
            };
            db.Class.Add(newClass);
            classes[row.Nickname] = newClass;
            classesCreated++;
        }

        await db.SaveChangesAsync();   // writes the inserts and the class changes
        await transaction.CommitAsync(); // everything above becomes visible together

        return new SaveResult(created, updated, unchanged, coursesCreated, classesCreated);
    }

    // The column in the database is "timestamp with time zone", and Npgsql refuses DateTime values
    // of unknown kind. Excel dates carry no time zone, so they are labelled UTC as they are.
    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
