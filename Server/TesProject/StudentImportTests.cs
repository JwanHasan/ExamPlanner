using DatabaseConnection.model;
using ExamPlannerServer.Import;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace TesProject;

// Opslagsark -> students: the file is recognised, routed to the student importer, and its students are saved.
// Every [Theory] runs on in-memory SQLite, and on real PostgreSQL too when EXAMPLANNER_TEST_PG is set.
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

    private static async Task<List<Student>> StudentsAsync(TestDatabase db)
    {
        await using var context = db.NewContext();
        return await context.Student.Include(s => s.User).AsNoTracking().OrderBy(s => s.SourceStudentId).ToListAsync();
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

    // ---------- saving students ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NewStudents_AreSaved_WithAUserThatCannotLogIn(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);

        var response = await ImportAsync(db, StudentWorkbook.Build(
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", attempts: 1),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26", attempts: 0)));

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
    public async Task SeveralRowsOfTheSameStudent_GiveOneStudent(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);

        var response = await ImportAsync(db, StudentWorkbook.Build(
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26"),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-CIE-S26"),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-NEG-S26")));

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

        var response = await ImportAsync(db, StudentWorkbook.Build(
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", attempts: 0),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-CIE-S26", attempts: 2),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-NEG-S26", attempts: 1),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26", attempts: 1),
            StudentWorkbook.ValidRow(student: "STU-0003", nickname: "KA61-BDL-S26", attempts: 3),
            StudentWorkbook.ValidRow(student: "STU-0003", nickname: "KA61-CIE-S26", attempts: 0)));

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

        var response = await ImportAsync(db, StudentWorkbook.Build(
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26", attempts: 2),
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-CIE-S26", attempts: 2)));

        Assert.Empty(response.Issues);
    }

    // ---------- importing again ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ReImport_CreatesNobodyNew_AndTheFileDecidesTheAttempt(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        await ImportAsync(db, StudentWorkbook.Build(StudentWorkbook.ValidRow(student: "STU-0001", attempts: 1)));

        var second = await ImportAsync(db, StudentWorkbook.Build(StudentWorkbook.ValidRow(student: "STU-0001", attempts: 3)));

        Assert.Equal(0, second.Created.Students);
        var student = Assert.Single(await StudentsAsync(db));
        Assert.Equal(3, student.AttemptNo);
        Assert.Equal(1, await UserCountAsync(db));

        await ImportAsync(db, StudentWorkbook.Build(StudentWorkbook.ValidRow(student: "STU-0001", attempts: 0)));
        Assert.Equal(0, Assert.Single(await StudentsAsync(db)).AttemptNo); // lower again: the file wins
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task AStudentThatAlreadyExists_IsReused_NotDuplicated(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        await using (var context = db.NewContext())
        {
            var user = new User { Email = "someone@students.invalid", Name = "Someone", Role = Role.Student };
            context.Student.Add(new Student { Id = 7, User = user, SourceStudentId = 1, AttemptNo = 0 });
            await context.SaveChangesAsync();
        }

        var response = await ImportAsync(db, StudentWorkbook.Build(StudentWorkbook.ValidRow(student: "STU-0001", attempts: 2)));

        Assert.Equal(0, response.Created.Students);
        var student = Assert.Single(await StudentsAsync(db));
        Assert.Equal(7, student.Id);
        Assert.Equal(2, student.AttemptNo);
        Assert.Equal("Someone", student.User.Name);   // the existing user was left as it was
        Assert.Equal(1, await UserCountAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NewStudentIds_DoNotClashWithExistingOnes(DbKind kind)
    {
        // Student.Id is not generated by the database, so the importer has to pick free ones.
        await using var db = await TestDatabase.CreateAsync(kind);
        await using (var context = db.NewContext())
        {
            var user = new User { Email = "old@students.invalid", Name = "Old", Role = Role.Student };
            context.Student.Add(new Student { Id = 10, User = user, SourceStudentId = 999, AttemptNo = 0 });
            await context.SaveChangesAsync();
        }

        await ImportAsync(db, StudentWorkbook.Build(
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26"),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26")));

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
        var bad = StudentWorkbook.WithValue(StudentWorkbook.UniqueRow(), StudentWorkbook.Attempts, "many");

        var response = await ImportAsync(db, StudentWorkbook.Build(StudentWorkbook.UniqueRow(), bad, StudentWorkbook.UniqueRow()));

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
    public async Task FailureWhileSaving_RollsBackAllStudentsAndUsers()
    {
        // SQLite only: the failure is forced with a trigger, which is written differently in PostgreSQL.
        await using var db = await TestDatabase.CreateAsync(DbKind.Sqlite);
        await using (var context = db.NewContext())
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TRIGGER fail_on_boom BEFORE INSERT ON \"User\" " +
                "WHEN NEW.\"Email\" = 'stu-0002@students.invalid' BEGIN SELECT RAISE(ABORT, 'boom'); END;");
        }

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ImportAsync(db, StudentWorkbook.Build(
            StudentWorkbook.ValidRow(student: "STU-0001", nickname: "KA61-BDL-S26"),
            StudentWorkbook.ValidRow(student: "STU-0002", nickname: "KA61-BDL-S26"),
            StudentWorkbook.ValidRow(student: "STU-0003", nickname: "KA61-BDL-S26"))));

        Assert.Empty(await StudentsAsync(db));          // not even the first student stays behind
        Assert.Equal(0, await UserCountAsync(db));
    }

    // ---------- the real Opslagsark ----------

    [RealOpslagsarkFact]
    public async Task RealOpslagsark_CreatesItsStudents_AndAReimportChangesNothing()
    {
        await using var db = await TestDatabase.CreateAsync(DbKind.Sqlite);

        await using (var file = new FileStream(OpslagsarkFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var first = await ImportAsync(db, file);

            Assert.Equal(6411, first.TotalRows);
            Assert.Equal(6411, first.ImportedRows);
            Assert.Equal(0, first.SkippedRows);
            Assert.Equal(1154, first.Created.Students);
            var warning = Assert.Single(first.Issues);
            Assert.Equal("warning", warning.Severity);
            Assert.Contains("238 student(s)", warning.Message);
            Assert.Contains("STU-0006", warning.Message);    // the first student whose rows disagree, in file order
        }

        var students = await StudentsAsync(db);
        Assert.Equal(1154, students.Count);
        Assert.Equal(Enumerable.Range(1, 1154), students.Select(s => s.SourceStudentId));
        Assert.All(students, s => Assert.Equal(Role.Student, s.User.Role));
        Assert.All(students, s => Assert.Equal("", s.User.HashedPassword));
        Assert.Equal(1154, students.Select(s => s.Id).Distinct().Count());
        // Worked out from the file independently of the code: students by their highest attempt.
        Assert.Equal(908, students.Count(s => s.AttemptNo == 0));
        Assert.Equal(103, students.Count(s => s.AttemptNo == 1));
        Assert.Equal(115, students.Count(s => s.AttemptNo == 2));
        Assert.Equal(22, students.Count(s => s.AttemptNo == 3));
        Assert.Equal(6, students.Count(s => s.AttemptNo == 4));

        await using var again = new FileStream(OpslagsarkFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var second = await ImportAsync(db, again);
        Assert.Equal(0, second.Created.Students);
        Assert.Equal(1154, (await StudentsAsync(db)).Count);
        Assert.Equal(1154, await UserCountAsync(db));
    }
}
