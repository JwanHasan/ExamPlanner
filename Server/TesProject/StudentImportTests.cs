using DatabaseConnection.model;
using ExamPlannerServer.Import;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace TesProject;

// Opslagsark -> students and enrollments: the file is recognised, routed to the student importer, and every
// row whose planning element is in the database is saved. Every [Theory] runs on in-memory SQLite, and on real
// PostgreSQL too when EXAMPLANNER_TEST_PG is set.
public class StudentImportTests
{
    public static IEnumerable<object[]> Databases() => TestDatabase.Kinds();

    // ---------- helpers ----------

    private static async Task<ImportResponse> ImportAsync(TestDatabase db, Stream workbook, string fileName = "opslagsark.xlsx")
    {
        await using var context = db.NewContext();
        return await TestServices.ImportService(context)
            .ImportAsync(new FormFile(workbook, 0, workbook.Length, "file", fileName));
    }

    // A row of the Opslagsark only imports when its planning element is already in the database, so the tests
    // first import planning elements with the same Kaldenavn, the way a planner would.
    private static async Task SeedElementsAsync(TestDatabase db, params object?[][] studentRows)
    {
        var column = Array.FindIndex(StudentWorkbook.Headers, h => h.StartsWith("Kaldenavn"));
        var elements = studentRows
            .Select(r => (string)r[column]!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(nickname => TestWorkbook.ValidRow(nickname: nickname))
            .ToArray();
        await ImportAsync(db, TestWorkbook.Build(elements), "export.xlsx");
    }

    private static async Task<List<Student>> StudentsAsync(TestDatabase db)
    {
        await using var context = db.NewContext();
        return await context.Student.Include(s => s.User).AsNoTracking().OrderBy(s => s.SourceStudentId).ToListAsync();
    }

    private static async Task<List<Enrollment>> EnrollmentsAsync(TestDatabase db)
    {
        await using var context = db.NewContext();
        return await context.Enrollment.Include(e => e.PlanningEducationElement).Include(e => e.Student).AsNoTracking().ToListAsync();
    }

    private static async Task<int> UserCountAsync(TestDatabase db)
    {
        await using var context = db.NewContext();
        return await context.User.CountAsync();
    }

    // ---------- recognising the file ----------

    [Fact]
    public void Detector_RecognisesAnOpslagsark_ByItsStudentColumn()
    {
        using var opslagsark = StudentWorkbook.Build(StudentWorkbook.ValidRow());

        Assert.True(ImportFileDetector.IsStudentRegistrations(opslagsark));
        Assert.Equal(0, opslagsark.Position); // rewound, so the importer can read the file from the start
    }

    [Fact]
    public void Detector_DoesNotTakeThePlanningElementExportForAnOpslagsark()
    {
        using var export = TestWorkbook.Build(TestWorkbook.ValidRow());

        Assert.False(ImportFileDetector.IsStudentRegistrations(export));
        Assert.Equal(0, export.Position);
    }

    [Fact]
    public void Detector_SaysNo_ForSomethingThatIsNotExcel()
    {
        using var notExcel = new MemoryStream("plain text, not an xlsx"u8.ToArray());

        Assert.False(ImportFileDetector.IsStudentRegistrations(notExcel));
        Assert.Equal(0, notExcel.Position);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task PlanningElementExport_StillGoesToThePlanningElementImporter(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);

        var response = await ImportAsync(db, TestWorkbook.Build(TestWorkbook.ValidRow()), "export.xlsx");

        Assert.Equal(1, response.Created.Courses);   // the planning-element importer made a course...
        Assert.Equal(0, response.Created.Students);  // ...and no students
        Assert.Empty(await StudentsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NotAnExcelFile_IsStillRejectedWithAMessage(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        using var notExcel = new MemoryStream("plain text, not an xlsx"u8.ToArray());

        await Assert.ThrowsAsync<ImportFileException>(() => ImportAsync(db, notExcel));
    }

    // ---------- saving students and enrollments ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NewStudents_AreSaved_WithAUserThatCannotLogIn(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var rows = new[]
        {
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", attempts: 1),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26", attempts: 0)
        };
        await SeedElementsAsync(db, rows);

        var response = await ImportAsync(db, StudentWorkbook.Build(rows));

        Assert.Equal(2, response.Created.Students);
        Assert.Equal(2, response.ImportedRows);
        Assert.Empty(response.Issues);

        var students = await StudentsAsync(db);
        Assert.Equal([1, 2], students.Select(s => s.SourceStudentId));
        Assert.Equal([1, 0], students.Select(s => s.AttemptNo));
        Assert.Equal(2, students.Select(s => s.Id).Distinct().Count());

        var user = students[0].User;
        Assert.Equal(Role.Student, user.Role);
        Assert.Equal("STU-0001", user.Name);
        Assert.Equal("stu-0001@students.invalid", user.Email);
        Assert.Equal("", user.HashedPassword);               // no password: nobody can log in as this user
        Assert.Equal(students[0].UserId, user.Id);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task EachRow_BecomesAnEnrollment_WithTheHoldFromTheFile(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var rows = new[]
        {
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", hold: "KA61-BDL-F26"),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-CIE-S26", hold: "KA61-CIE-F26"),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26", hold: "KA61-BDL-F27")
        };
        await SeedElementsAsync(db, rows);

        var response = await ImportAsync(db, StudentWorkbook.Build(rows));

        Assert.Equal(3, response.Created.Enrollments);
        Assert.Equal(2, response.Created.Students);                    // STU-0001 has two rows but is one student
        var enrollments = await EnrollmentsAsync(db);
        Assert.Equal(3, enrollments.Count);
        Assert.Equal(["KA61-BDL-F26", "KA61-BDL-F27", "KA61-CIE-F26"], enrollments.Select(e => e.Hold).Order());
        Assert.All(enrollments, e => Assert.Equal(0, e.SourceImportId));
        // Each enrollment points at the right planning element and the right student.
        var first = enrollments.Single(e => e.Hold == "KA61-CIE-F26");
        Assert.Equal("KA61-CIE-S26", first.PlanningEducationElement.Nickmane);
        Assert.Equal(1, first.Student.SourceStudentId);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task SeveralRowsOfTheSameStudent_GiveOneStudentAndOneUser(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var rows = new[]
        {
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26"),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-CIE-S26"),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-NEG-S26")
        };
        await SeedElementsAsync(db, rows);

        var response = await ImportAsync(db, StudentWorkbook.Build(rows));

        Assert.Equal(1, response.Created.Students);
        Assert.Equal(3, response.ImportedRows);
        Assert.Single(await StudentsAsync(db));
        Assert.Equal(1, await UserCountAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task TheHighestAttempt_IsSaved_AndOneWarningTellsThatRowsDisagreed(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var rows = new[]
        {
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", attempts: 0),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-CIE-S26", attempts: 2),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-NEG-S26", attempts: 1),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26", attempts: 1),
            StudentWorkbook.ValidRow(student: "STU-0003", nickname: "KA61-BDL-S26", attempts: 3),
            StudentWorkbook.ValidRow(student: "STU-0003", nickname: "KA61-CIE-S26", attempts: 0)
        };
        await SeedElementsAsync(db, rows);

        var response = await ImportAsync(db, StudentWorkbook.Build(rows));

        Assert.Equal([2, 1, 3], (await StudentsAsync(db)).Select(s => s.AttemptNo));

        var warning = Assert.Single(response.Issues);                 // one for the whole file, not one per student
        Assert.Equal("warning", warning.Severity);
        Assert.Equal(0, warning.Row);                                 // row 0 = the file as a whole
        Assert.Equal("Antal forsøg", warning.Column);
        Assert.Contains("2 student(s)", warning.Message);             // STU-0001 and STU-0003
        Assert.Contains("STU-0001", warning.Message);
        Assert.Equal(0, response.SkippedRows);                        // a warning is not a skipped row
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NoWarning_WhenAStudentsRowsAgree(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var rows = new[]
        {
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", attempts: 2),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-CIE-S26", attempts: 2)
        };
        await SeedElementsAsync(db, rows);

        var response = await ImportAsync(db, StudentWorkbook.Build(rows));

        Assert.Empty(response.Issues);
    }

    // ---------- rows whose planning element is not there ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task RowWithoutItsPlanningElement_IsSkipped_AndSavesNothingNotEvenItsStudent(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var known = StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26");
        var unknown = StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-UNKNOWN-S26");
        await SeedElementsAsync(db, known);                       // only the first element exists

        var response = await ImportAsync(db, StudentWorkbook.Build(known, unknown));

        Assert.Equal(2, response.TotalRows);
        Assert.Equal(1, response.ImportedRows);
        Assert.Equal(1, response.SkippedRows);
        Assert.Equal(1, response.Created.Students);               // STU-0002 only had the skipped row
        Assert.Equal(1, response.Created.Enrollments);
        Assert.Equal([1], (await StudentsAsync(db)).Select(s => s.SourceStudentId));

        var issue = Assert.Single(response.Issues);
        Assert.Equal("error", issue.Severity);
        Assert.Equal(3, issue.Row);                               // the second data row
        Assert.Equal("Kaldenavn (Planlægningsuddannelseselement)", issue.Column);
        Assert.Contains("KA61-UNKNOWN-S26", issue.Message);
        Assert.Contains("planning-element export", issue.Message);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task AMissingPlanningElement_IsReportedOnceForAllItsRows(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var rows = new[]
        {
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-GONE-S26"),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-GONE-S26"),
            StudentWorkbook.ValidRow(student: "STU-0003", nickname: "KA61-GONE-S26")
        };

        var response = await ImportAsync(db, StudentWorkbook.Build(rows));

        Assert.Equal(0, response.ImportedRows);
        Assert.Equal(3, response.SkippedRows);                    // all three rows are counted as skipped
        var issue = Assert.Single(response.Issues);               // but there is one line about the element
        Assert.Equal(2, issue.Row);                               // pointing at its first row
        Assert.Contains("3 row(s)", issue.Message);
        Assert.Empty(await StudentsAsync(db));
        Assert.Empty(await EnrollmentsAsync(db));
        Assert.Equal(0, await UserCountAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task TheKaldenavnIsMatchedIgnoringCapitalisation(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        await SeedElementsAsync(db, StudentWorkbook.ValidRow(nickname: "KA61-BDL-S26"));

        var response = await ImportAsync(db, StudentWorkbook.Build(StudentWorkbook.ValidRow(nickname: "ka61-bdl-s26")));

        Assert.Equal(1, response.ImportedRows);
        Assert.Single(await EnrollmentsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task AKaldenavnThatMatchesTwoPlanningElements_IsSkipped_NotGuessed(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        // Two planning elements (different ids) with the same Kaldenavn.
        await ImportAsync(db, TestWorkbook.Build(
            TestWorkbook.ValidRow(nickname: "KA61-TWIN-S26"), TestWorkbook.ValidRow(nickname: "KA61-TWIN-S26")), "export.xlsx");

        var response = await ImportAsync(db, StudentWorkbook.Build(StudentWorkbook.ValidRow(nickname: "KA61-TWIN-S26")));

        Assert.Equal(0, response.ImportedRows);
        Assert.Equal(1, response.SkippedRows);
        Assert.Contains("matches 2 planning elements", Assert.Single(response.Issues).Message);
        Assert.Empty(await EnrollmentsAsync(db));
    }

    // ---------- importing again ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ReImport_CreatesNothingNew_AndTheFileDecidesTheAttemptAndTheHold(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var before = StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", hold: "OLD-HOLD", attempts: 1);
        await SeedElementsAsync(db, before);
        await ImportAsync(db, StudentWorkbook.Build(before));

        var after = StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", hold: "NEW-HOLD", attempts: 3);
        var second = await ImportAsync(db, StudentWorkbook.Build(after));

        Assert.Equal(0, second.Created.Students);
        Assert.Equal(0, second.Created.Enrollments);
        Assert.Equal(1, second.ImportedRows);                       // the row was processed, it just created nothing new
        Assert.Equal(3, Assert.Single(await StudentsAsync(db)).AttemptNo);
        Assert.Equal("NEW-HOLD", Assert.Single(await EnrollmentsAsync(db)).Hold);
        Assert.Equal(1, await UserCountAsync(db));

        await ImportAsync(db, StudentWorkbook.Build(StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", attempts: 0)));
        Assert.Equal(0, Assert.Single(await StudentsAsync(db)).AttemptNo); // lower again: the file wins
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ARowThatCouldNotBeImportedBefore_IsImportedOnceItsElementExists(DbKind kind)
    {
        // The normal order of events: Opslagsark first (everything skipped), planning elements later, Opslagsark again.
        await using var db = await TestDatabase.CreateAsync(kind);
        var rows = new[] { StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-LATE-S26") };

        var first = await ImportAsync(db, StudentWorkbook.Build(rows));
        Assert.Equal(0, first.ImportedRows);

        await SeedElementsAsync(db, rows);
        var second = await ImportAsync(db, StudentWorkbook.Build(rows));

        Assert.Equal(1, second.ImportedRows);
        Assert.Equal(1, second.Created.Students);
        Assert.Equal(1, second.Created.Enrollments);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task AStudentThatAlreadyExists_IsReused_NotDuplicated(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var row = StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", attempts: 2);
        await SeedElementsAsync(db, row);
        await using (var context = db.NewContext())
        {
            var user = new User { Email = "someone@students.invalid", Name = "Someone", Role = Role.Student };
            context.Student.Add(new Student { Id = 7, User = user, SourceStudentId = 1, AttemptNo = 0 });
            await context.SaveChangesAsync();
        }

        var response = await ImportAsync(db, StudentWorkbook.Build(row));

        Assert.Equal(0, response.Created.Students);
        Assert.Equal(1, response.Created.Enrollments);              // but the new student-on-element link is created
        var student = Assert.Single(await StudentsAsync(db));
        Assert.Equal(7, student.Id);
        Assert.Equal(2, student.AttemptNo);
        Assert.Equal("Someone", student.User.Name);                 // the existing user was left as it was
        Assert.Equal(1, await UserCountAsync(db));
        Assert.Equal(7, Assert.Single(await EnrollmentsAsync(db)).StudentId);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NewStudentIds_DoNotClashWithExistingOnes(DbKind kind)
    {
        // Student.Id is not generated by the database, so the importer has to pick free ones.
        var rows = new[]
        {
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26"),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26")
        };
        await using var db = await TestDatabase.CreateAsync(kind);
        await SeedElementsAsync(db, rows);
        await using (var context = db.NewContext())
        {
            var user = new User { Email = "old@students.invalid", Name = "Old", Role = Role.Student };
            context.Student.Add(new Student { Id = 10, User = user, SourceStudentId = 999, AttemptNo = 0 });
            await context.SaveChangesAsync();
        }

        await ImportAsync(db, StudentWorkbook.Build(rows));

        var students = await StudentsAsync(db);
        Assert.Equal(3, students.Count);
        Assert.Equal([10, 11, 12], students.Select(s => s.Id).Order());
    }

    // ---------- bad rows and bad files ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task BadRow_IsSkippedAndReported_OthersAreImported(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var good1 = StudentWorkbook.UniqueRow();
        var bad = StudentWorkbook.WithValue(StudentWorkbook.UniqueRow(), StudentWorkbook.Attempts, "many");
        var good2 = StudentWorkbook.UniqueRow();
        await SeedElementsAsync(db, good1, bad, good2);

        var response = await ImportAsync(db, StudentWorkbook.Build(good1, bad, good2));

        Assert.Equal(3, response.TotalRows);
        Assert.Equal(2, response.ImportedRows);
        Assert.Equal(1, response.SkippedRows);
        Assert.Equal(2, response.Created.Students);                 // nobody is created for the bad row
        var issue = Assert.Single(response.Issues);
        Assert.Equal("error", issue.Severity);
        Assert.Equal("Data", issue.Sheet);
        Assert.Equal(3, issue.Row);
        Assert.Contains("Antal forsøg", issue.Message);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task MissingColumn_ThrowsImportFileException_AndSavesNothing(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var index = Array.FindIndex(StudentWorkbook.Headers, h => h == "Hold");
        var headers = StudentWorkbook.Headers.Where((_, i) => i != index).ToArray();
        var row = StudentWorkbook.ValidRow().Where((_, i) => i != index).ToArray();

        var error = await Assert.ThrowsAsync<ImportFileException>(() => ImportAsync(db, TestWorkbook.Build(headers, row)));

        Assert.Contains("Hold", error.Message);
        Assert.Empty(await StudentsAsync(db));
        Assert.Equal(0, await UserCountAsync(db));
    }

    [Fact]
    public async Task FailureWhileSaving_RollsBackStudentsUsersAndEnrollments()
    {
        // SQLite only: the failure is forced with a trigger, which is written differently in PostgreSQL.
        await using var db = await TestDatabase.CreateAsync(DbKind.Sqlite);
        var rows = new[]
        {
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", hold: "fine"),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26", hold: "boom"),
            StudentWorkbook.ValidRow(student: "STU-0003", nickname: "KA61-BDL-S26", hold: "fine")
        };
        await SeedElementsAsync(db, rows);
        await using (var context = db.NewContext())
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TRIGGER fail_on_boom BEFORE INSERT ON \"Enrollment\" " +
                "WHEN NEW.\"Hold\" = 'boom' BEGIN SELECT RAISE(ABORT, 'boom'); END;");
        }

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ImportAsync(db, StudentWorkbook.Build(rows)));

        Assert.Empty(await StudentsAsync(db));          // not even the students saved before the failing row
        Assert.Empty(await EnrollmentsAsync(db));
        Assert.Equal(0, await UserCountAsync(db));
    }

    // ---------- the real files together ----------

    [RealBothFilesFact]
    public async Task RealFiles_PlanningElementsFirst_ThenTheOpslagsark_AndAReimportChangesNothing()
    {
        await using var db = await TestDatabase.CreateAsync(DbKind.Sqlite);

        // 1. The planning-element export: 108 elements.
        await using (var export = new FileStream(ExcelTest.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            Assert.Equal(108, (await ImportAsync(db, export, "export.xlsx")).ImportedRows);

        // 2. The Opslagsark: only the rows on those 108 elements can be imported.
        await using (var file = new FileStream(OpslagsarkFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var first = await ImportAsync(db, file);

            // Worked out from the two files independently of the code.
            Assert.Equal(6411, first.TotalRows);
            Assert.Equal(2506, first.ImportedRows);          // rows whose Kaldenavn is one of the 108 elements
            Assert.Equal(3905, first.SkippedRows);
            Assert.Equal(623, first.Created.Students);       // students with at least one of those rows
            Assert.Equal(2506, first.Created.Enrollments);   // no student is on the same element twice

            // 267 elements are missing: one line each, and then one warning about attempts.
            Assert.Equal(267, first.Issues.Count(i => i.Severity == "error"));
            var warning = Assert.Single(first.Issues, i => i.Severity == "warning");
            Assert.Contains("122 student(s)", warning.Message);   // students whose IMPORTED rows disagree
            Assert.Contains("STU-0017", warning.Message);

            var firstMissing = first.Issues.First(i => i.Severity == "error");
            Assert.Equal(2, firstMissing.Row);                    // sheet row 2 is the first row of an unknown element
            Assert.Contains("KA61-BDL-S26", firstMissing.Message);
            Assert.Contains("13 row(s)", firstMissing.Message);
        }

        var students = await StudentsAsync(db);
        Assert.Equal(623, students.Count);
        Assert.All(students, s => Assert.Equal(Role.Student, s.User.Role));
        Assert.All(students, s => Assert.Equal("", s.User.HashedPassword));
        Assert.Equal(491, students.Count(s => s.AttemptNo == 0));   // by highest attempt on the imported rows
        Assert.Equal(60, students.Count(s => s.AttemptNo == 1));
        Assert.Equal(68, students.Count(s => s.AttemptNo == 2));
        Assert.Equal(3, students.Count(s => s.AttemptNo == 3));
        Assert.Equal(1, students.Count(s => s.AttemptNo == 4));

        var enrollments = await EnrollmentsAsync(db);
        Assert.Equal(2506, enrollments.Count);
        var firstEnrollment = enrollments.Single(e => e.Student.SourceStudentId == 2 && e.PlanningEducationElement.Nickmane == "TS-DBS1-S26");
        Assert.Equal("TS-DBS1-S26", firstEnrollment.Hold);          // sheet row 8, the first row that finds its element

        await using var again = new FileStream(OpslagsarkFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var second = await ImportAsync(db, again);
        Assert.Equal(0, second.Created.Students);
        Assert.Equal(0, second.Created.Enrollments);
        Assert.Equal(2506, second.ImportedRows);
        Assert.Equal(623, (await StudentsAsync(db)).Count);
        Assert.Equal(2506, (await EnrollmentsAsync(db)).Count);
        Assert.Equal(623, await UserCountAsync(db));
    }
}
