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

public class ImportService(
    IExcelImportParser parser,
    AppDbContext db,
    ILogger<ImportService> logger
) : IImportService
{
    private sealed record ValidRow(
        ParsedPlanningEducationElementRow Row,
        PlanningEducationElement.Assessment Assessment,
        string Prefix,
        string CourseCode
    );

    private sealed record SaveResult(
        int Created,
        int Updated,
        int Unchanged,
        int CoursesCreated,
        int ClassesCreated
    );

    public async Task<ImportResponse> ImportAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();

        var parsed = parser.Parse(stream);

        var fileProblem = parsed.Issues
            .FirstOrDefault(i => i.Row == 0);

        if (fileProblem is not null)
            throw new ImportFileException(fileProblem.Message);

        var issues = parsed.Issues
            .Select(i => new ImportIssue(
                "error",
                parsed.SheetName,
                i.Row,
                null,
                i.Message
            ))
            .ToList();

        var skipped = issues.Count;

        var valid = new List<ValidRow>();

        foreach (var row in parsed.Rows)
        {
            var problem = FindDatabaseProblem(
                row,
                out var assessment,
                out var prefix,
                out var courseCode
            );

            if (problem is not null)
            {
                issues.Add(new ImportIssue(
                    "error",
                    parsed.SheetName,
                    row.SourceRowNumber,
                    problem.Value.Column,
                    problem.Value.Message
                ));

                skipped++;

                continue;
            }

            valid.Add(new ValidRow(
                row,
                assessment,
                prefix!,
                courseCode!
            ));

            if (CourseDerivation.Campus(
                    row.ActivityOffering
                ).Length == 0)
            {
                issues.Add(new ImportIssue(
                    "warning",
                    parsed.SheetName,
                    row.SourceRowNumber,
                    "Aktivitetsudbud",
                    "No campus found in the text, so the planning element uses an Unknown campus."
                ));
            }
        }

        var saved = await SaveAsync(valid);

        var imported =
            saved.Created +
            saved.Updated +
            saved.Unchanged;

        logger.LogInformation(
            "Import of {File}: {Created} elements created, " +
            "{Updated} updated, {Unchanged} unchanged, " +
            "{Courses} courses and {Classes} classes created, " +
            "{Skipped} rows skipped",

            file.FileName,
            saved.Created,
            saved.Updated,
            saved.Unchanged,
            saved.CoursesCreated,
            saved.ClassesCreated,
            skipped
        );

        return new ImportResponse
        {
            FileName = file.FileName,
            TotalRows = imported + skipped,
            ImportedRows = imported,
            SkippedRows = skipped,

            Created = new ImportCounts
            {
                Courses = saved.CoursesCreated,
                Classes = saved.ClassesCreated
            },

            Issues = issues
        };
    }

    private static (string Column, string Message)?
        FindDatabaseProblem(
            ParsedPlanningEducationElementRow row,
            out PlanningEducationElement.Assessment assessment,
            out string? prefix,
            out string? courseCode
        )
    {
        assessment = default;

        prefix = CourseDerivation.Prefix(row.Nickname);

        courseCode = CourseDerivation.CourseCode(row.Name);

        if (row.Ects % 1 != 0)
        {
            return (
                "ECTS",
                $"ECTS {row.Ects} has decimals, but the database stores whole numbers."
            );
        }

        if (prefix is null)
        {
            return (
                "Kaldenavn",
                $"'{row.Nickname}' does not start with a two-letter programme prefix."
            );
        }

        if (courseCode is null)
        {
            return (
                "Navn",
                $"No course code found at the start of '{row.Name}'."
            );
        }

        switch (row.AssessmentType.Trim().ToLowerInvariant())
        {
            case "intern":
                assessment =
                    PlanningEducationElement.Assessment.Internal;

                return null;

            case "ekstern":
                assessment =
                    PlanningEducationElement.Assessment.External;

                return null;

            default:
                return (
                    "Bedømmelsestype",
                    $"'{row.AssessmentType}' is not Intern or Ekstern."
                );
        }
    }

    private async Task<SaveResult> SaveAsync(
        List<ValidRow> valid
    )
    {
        int created = 0;
        int updated = 0;
        int unchanged = 0;
        int coursesCreated = 0;
        int classesCreated = 0;

        var ids = valid
            .Select(v => v.Row.PlanningElementId)
            .ToList();

        var existing = await db.PlanningEducationElement
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        var classCodes = valid
            .Select(v => v.Row.Nickname)
            .Distinct()
            .ToList();

        var classes = (
                await db.Class
                    .Where(c => classCodes.Contains(c.ClassCode))
                    .ToListAsync()
            )
            .GroupBy(c => c.ClassCode)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(c => c.Id).First()
            );

        var prefixes = valid
            .Select(v => v.Prefix)
            .Distinct()
            .ToList();

        var codes = valid
            .Select(v => v.CourseCode)
            .Distinct()
            .ToList();

        var courses = (
                await db.Course
                    .Where(c =>
                        prefixes.Contains(c.Prefix) &&
                        codes.Contains(c.SueCode)
                    )
                    .ToListAsync()
            )
            .GroupBy(c =>
                CourseDerivation.CourseKey(
                    c.Prefix,
                    c.SueCode
                )
            )
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(c => c.Id).First()
            );

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var campusRows = await db.Campuses.ToListAsync();

        var campuses = campusRows
            .GroupBy(
                c => c.Name,
                StringComparer.OrdinalIgnoreCase
            )
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(c => c.Id).First(),
                StringComparer.OrdinalIgnoreCase
            );

        var parents =
            new Dictionary<Guid, (Class Class, Campus Campus)>();

        foreach (var item in valid)
        {
            var (row, _, prefix, courseCode) = item;

            var campusName =
                CourseDerivation.Campus(row.ActivityOffering);

            var isVirtual =
                row.ActivityOffering.Contains(
                    "(online)",
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                row.ActivityOffering.Contains(
                    "(e-læring)",
                    StringComparison.OrdinalIgnoreCase
                );

            if (campusName.Length == 0)
                campusName = "Unknown";

            if (isVirtual)
                campusName += " (e-læring)";

            if (!campuses.TryGetValue(
                    campusName,
                    out var campus
                ))
            {
                campus = new Campus
                {
                    Name = campusName,
                    IsVirtual = isVirtual
                };

                db.Campuses.Add(campus);

                campuses[campusName] = campus;
            }

            if (!classes.TryGetValue(
                    row.Nickname,
                    out var cls
                ))
            {
                var key = CourseDerivation.CourseKey(
                    prefix,
                    courseCode
                );

                if (!courses.TryGetValue(
                        key,
                        out var course
                    ))
                {
                    course = new Course
                    {
                        SueCode = courseCode,
                        Prefix = prefix,

                        Name =
                            CourseDerivation.CourseName(row.Name),

                        ECTS = (int)row.Ects
                    };

                    db.Course.Add(course);

                    courses[key] = course;

                    coursesCreated++;
                }

                cls = new Class
                {
                    ClassCode = row.Nickname,
                    Course = course
                };

                db.Class.Add(cls);

                classes[row.Nickname] = cls;

                classesCreated++;
            }

            cls.NickName = row.Nickname;
            cls.StartDate = AsUtc(row.StartDate);
            cls.EndDate = AsUtc(row.EndDate);

            parents[row.PlanningElementId] = (cls, campus);
        }

        // Obtain the IDs required by the planning elements.
        await db.SaveChangesAsync();

        foreach (var item in valid)
        {
            var row = item.Row;

            var assessment = item.Assessment;

            var startDate =
                DateOnly.FromDateTime(row.StartDate);

            var endDate =
                DateOnly.FromDateTime(row.EndDate);

            var modifiedAt =
                AsUtc(row.SourceModifiedAt);

            var parent =
                parents[row.PlanningElementId];

            if (!existing.TryGetValue(
                    row.PlanningElementId,
                    out var current
                ))
            {
                db.PlanningEducationElement.Add(
                    new PlanningEducationElement
                    {
                        Id = row.PlanningElementId,

                        CourseId = parent.Class.CourseId,

                        CampusId = parent.Campus.Id,

                        SourceChecksum = row.SourceChecksum,

                        SourceModifiedAt = modifiedAt,

                        Name = row.Name,

                        Nickname = row.Nickname,

                        ECTS = (int)row.Ects,

                        ExecutionElementCount =
                            row.ExecutionElementCount,

                        StartDate = startDate,

                        EndDate = endDate,

                        ActivityOffering = row.ActivityOffering,

                        AssessmentType = assessment,

                        CompletionAssessment =
                            row.CompletionAssessment,

                        GradingScale = row.GradingScale,

                        CombinedWrittenOral =
                            row.CombinedWrittenOral,

                        Oral = row.Oral,

                        PracticalExam = row.PracticalExam,

                        Project = row.Project,

                        Written = row.Written
                    }
                );

                created++;
            }
            else if (current.SourceChecksum ==
                     row.SourceChecksum)
            {
                unchanged++;
            }
            else
            {
                await db.PlanningEducationElement
                    .Where(e =>
                        e.Id == row.PlanningElementId
                    )
                    .ExecuteUpdateAsync(s => s

                        .SetProperty(
                            e => e.SourceChecksum,
                            row.SourceChecksum
                        )

                        .SetProperty(
                            e => e.SourceModifiedAt,
                            modifiedAt
                        )

                        .SetProperty(
                            e => e.Name,
                            row.Name
                        )

                        .SetProperty(
                            e => e.Nickname,
                            row.Nickname
                        )

                        .SetProperty(
                            e => e.ECTS,
                            (int)row.Ects
                        )

                        .SetProperty(
                            e => e.ExecutionElementCount,
                            row.ExecutionElementCount
                        )

                        .SetProperty(
                            e => e.StartDate,
                            startDate
                        )

                        .SetProperty(
                            e => e.EndDate,
                            endDate
                        )

                        .SetProperty(
                            e => e.ActivityOffering,
                            row.ActivityOffering
                        )

                        .SetProperty(
                            e => e.AssessmentType,
                            assessment
                        )

                        .SetProperty(
                            e => e.CompletionAssessment,
                            row.CompletionAssessment
                        )

                        .SetProperty(
                            e => e.GradingScale,
                            row.GradingScale
                        )

                        .SetProperty(
                            e => e.CombinedWrittenOral,
                            row.CombinedWrittenOral
                        )

                        .SetProperty(
                            e => e.Oral,
                            row.Oral
                        )

                        .SetProperty(
                            e => e.PracticalExam,
                            row.PracticalExam
                        )

                        .SetProperty(
                            e => e.Project,
                            row.Project
                        )

                        .SetProperty(
                            e => e.Written,
                            row.Written
                        )
                    );

                updated++;
            }
        }

        await db.SaveChangesAsync();

        await transaction.CommitAsync();

        return new SaveResult(
            created,
            updated,
            unchanged,
            coursesCreated,
            classesCreated
        );
    }

    private static DateTime AsUtc(DateTime value)
    {
        return DateTime.SpecifyKind(
            value,
            DateTimeKind.Utc
        );
    }
}