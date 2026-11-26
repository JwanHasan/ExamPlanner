using DatabaseConnection.DBContext;
using DatabaseConnection.model;
using Microsoft.EntityFrameworkCore;

namespace ExamPlannerServer.Import;

/// Imports the Opslagsark: one row per student per planning element.
///
/// A row is only imported when its planning element is already in the database. The file has no element id,
/// so the row finds its element by Kaldenavn. Rows whose element is missing (import the planning-element export
/// first) are skipped and reported, and a skipped row saves nothing, not even its student.
///
/// Each imported row becomes an Enrollment (student + planning element + the student's Hold). Each student is
/// saved once, however many rows they have. A Student needs a User in the database, so each new student also gets
/// a User with the Student role and NO password: the login code refuses accounts without a password, so these
/// accounts can never be used to log in.
///
/// Students are matched on the number in their anonymous id ("STU-0001" -> 1), and enrollments on student +
/// planning element, so importing the same file again creates nothing twice. Everything is saved in one go.
public class StudentRegistrationImporter(
    IStudentRegistrationParser parser, AppDbContext db, ILogger<StudentRegistrationImporter> logger)
{
    private sealed record MatchedRow(ParsedStudentRegistrationRow Row, Guid PlanningElementId);

    public async Task<ImportResponse> ImportAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var parsed = parser.Parse(stream);

        // A problem with the whole file: ImportController answers 422 INVALID_IMPORT_DATA.
        var fileProblem = parsed.Issues.FirstOrDefault(i => i.Row == 0);
        if (fileProblem is not null)
            throw new ImportFileException(fileProblem.Message);

        // Rows the parser already rejected (one issue per row, with its row number).
        var issues = parsed.Issues
            .Select(i => new ImportIssue("error", parsed.SheetName, i.Row, null, i.Message))
            .ToList();
        var skipped = issues.Count;

        // Rows that are fine in themselves, but whose planning element is not in the database (or not clearly one).
        var matched = await MatchToPlanningElementsAsync(parsed.Rows, parsed.SheetName, issues);
        skipped += parsed.Rows.Count - matched.Count;

        var (studentsCreated, enrollmentsCreated) = await SaveAsync(matched, parsed.SheetName, issues);

        logger.LogInformation(
            "Import of {File}: {Students} students and {Enrollments} enrollments created, {Rows} rows imported, {Skipped} rows skipped",
            file.FileName, studentsCreated, enrollmentsCreated, matched.Count, skipped);

        return new ImportResponse
        {
            FileName = file.FileName,
            TotalRows = matched.Count + skipped,
            ImportedRows = matched.Count,
            SkippedRows = skipped,
            Created = new ImportCounts { Students = studentsCreated, Enrollments = enrollmentsCreated },
            Issues = issues
        };
    }

    // Finds each row's planning element by its Kaldenavn (capitalisation does not matter). A missing or unclear
    // element is reported ONCE for all its rows: in the real file thousands of rows would otherwise give thousands of lines.
    private async Task<List<MatchedRow>> MatchToPlanningElementsAsync(
        List<ParsedStudentRegistrationRow> rows, string sheetName, List<ImportIssue> issues)
    {
        var wanted = rows.Select(r => r.PlanningElementNickname.ToLowerInvariant()).Distinct().ToList();
        var found = await db.PlanningEducationElement
            .AsNoTracking()
            .Where(e => wanted.Contains(e.Nickmane.ToLower()))
            .Select(e => new { e.Id, e.Nickmane })
            .ToListAsync();
        var idsByNickname = found
            .GroupBy(e => e.Nickmane, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Id).ToList(), StringComparer.OrdinalIgnoreCase);

        var matched = new List<MatchedRow>();
        foreach (var group in rows.GroupBy(r => r.PlanningElementNickname, StringComparer.OrdinalIgnoreCase))
        {
            idsByNickname.TryGetValue(group.Key, out var ids);
            var firstRow = group.Min(r => r.SourceRowNumber);

            if (ids is null)
            {
                issues.Add(new ImportIssue("error", sheetName, firstRow, StudentRegistrationParser.ColElementNickname,
                    $"No planning element with Kaldenavn '{group.First().PlanningElementNickname}' is in the database yet " +
                    $"({group.Count()} row(s) skipped). Import the planning-element export first."));
            }
            else if (ids.Count > 1)
            {
                issues.Add(new ImportIssue("error", sheetName, firstRow, StudentRegistrationParser.ColElementNickname,
                    $"'{group.First().PlanningElementNickname}' matches {ids.Count} planning elements, so it is unclear " +
                    $"which one it means ({group.Count()} row(s) skipped)."));
            }
            else
            {
                matched.AddRange(group.Select(r => new MatchedRow(r, ids[0])));
            }
        }

        // Back in the order of the sheet (the rows were handled element by element above), so that the students
        // get their ids, and the warnings name their examples, in the order a planner sees them in the file.
        return matched.OrderBy(m => m.Row.SourceRowNumber).ToList();
    }

    private async Task<(int Students, int Enrollments)> SaveAsync(
        List<MatchedRow> matched, string sheetName, List<ImportIssue> issues)
    {
        var byNumber = matched.GroupBy(m => m.Row.StudentNumber).ToList();
        var numbers = byNumber.Select(g => g.Key).ToList();

        var existing = (await db.Student.Where(s => numbers.Contains(s.SourceStudentId)).ToListAsync())
            .GroupBy(s => s.SourceStudentId)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Id).First());

        // The enrollments that students we already have may already have (new students have none).
        var existingIds = existing.Values.Select(s => s.Id).ToList();
        var enrollments = (await db.Enrollment.Where(e => existingIds.Contains(e.StudentId)).ToListAsync())
            .ToDictionary(e => (e.StudentId, e.PlanningElementId));

        // Student.Id is not generated by the database, so the next free one is worked out here.
        var nextId = (await db.Student.MaxAsync(s => (int?)s.Id) ?? 0) + 1;

        int studentsCreated = 0, enrollmentsCreated = 0;
        var disagreeing = new List<string>();

        foreach (var group in byNumber)
        {
            // The database keeps ONE attempt number per student, but the file has one per row.
            // The highest is saved, and the planner is told when a student's rows do not agree.
            var attempts = group.Select(m => m.Row.AttemptNo).ToList();
            var highest = attempts.Max();
            if (attempts.Distinct().Count() > 1)
                disagreeing.Add(group.First().Row.StudentCode);

            if (existing.TryGetValue(group.Key, out var student))
            {
                student.AttemptNo = highest; // the file is the source: it wins
            }
            else
            {
                var code = group.First().Row.StudentCode;
                var user = new User
                {
                    Email = $"{code.ToLowerInvariant()}@students.invalid",   // ".invalid" can never be a real address
                    Name = code,
                    HashedPassword = "",                                      // no password: this account cannot log in
                    Role = Role.Student
                };
                student = new Student { Id = nextId++, User = user, SourceStudentId = group.Key, AttemptNo = highest };
                db.Student.Add(student);
                studentsCreated++;
            }

            foreach (var m in group)
            {
                if (enrollments.TryGetValue((student.Id, m.PlanningElementId), out var enrollment))
                {
                    enrollment.Hold = m.Row.Hold; // the file is the source: it wins
                    continue;
                }

                db.Enrollment.Add(new Enrollment
                {
                    StudentId = student.Id,
                    PlanningElementId = m.PlanningElementId,
                    Hold = m.Row.Hold
                    // SourceImportId stays 0: the database has no import history to point at yet.
                });
                enrollmentsCreated++;
            }
        }

        if (disagreeing.Count > 0)
        {
            // One warning for the whole file (row 0), not one per student: in the real file that would be hundreds.
            issues.Add(new ImportIssue("warning", sheetName, 0, "Antal forsøg",
                $"{disagreeing.Count} student(s) have different 'Antal forsøg' on different rows, and the database keeps one number " +
                $"per student, so the highest was saved. For example: {string.Join(", ", disagreeing.Take(3))}."));
        }

        // One SaveChanges is one transaction: if anything fails, no student, user or enrollment is left behind.
        await db.SaveChangesAsync();

        return (studentsCreated, enrollmentsCreated);
    }
}
