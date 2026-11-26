using DatabaseConnection.model;
using ExamPlannerServer.Import;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace TesProject;

// Service + database: ImportService reading a real .xlsx and saving through EF Core.
// Every [Theory] runs on in-memory SQLite, and on real PostgreSQL too when
// EXAMPLANNER_TEST_PG is set (see TestDatabase).
public class ImportServiceTests
{
    public static IEnumerable<object[]> Databases() =>
        TestDatabase.Kinds();

    private static async Task<ImportResponse> ImportAsync(
        TestDatabase db,
        Stream workbook,
        string fileName = "export.xlsx"
    )
    {
        await using var context = db.NewContext();

        var service = new ImportService(
            new ExcelImportParser(),
            context,
            NullLogger<ImportService>.Instance
        );

        return await service.ImportAsync(
            new FormFile(
                workbook,
                0,
                workbook.Length,
                "file",
                fileName
            )
        );
    }

    private static async Task<List<PlanningEducationElement>>
        RowsAsync(TestDatabase db)
    {
        await using var context = db.NewContext();

        return await context.PlanningEducationElement
            .AsNoTracking()
            .OrderBy(e => e.Nickname)
            .ToListAsync();
    }

    private static async Task<PlanningEducationElement>
        RowAsync(TestDatabase db, Guid id)
    {
        await using var context = db.NewContext();

        return await context.PlanningEducationElement
            .AsNoTracking()
            .SingleAsync(e => e.Id == id);
    }

    private static async Task<List<Course>>
        CoursesAsync(TestDatabase db)
    {
        await using var context = db.NewContext();

        return await context.Course
            .AsNoTracking()
            .OrderBy(c => c.Prefix)
            .ThenBy(c => c.SueCode)
            .ToListAsync();
    }

    private static async Task<List<Class>>
        ClassesAsync(TestDatabase db)
    {
        await using var context = db.NewContext();

        return await context.Class
            .AsNoTracking()
            .OrderBy(c => c.ClassCode)
            .ToListAsync();
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NewRow_IsInserted_WithAllFieldsMapped(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var id = Guid.NewGuid();

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(TestWorkbook.ValidRow(id))
        );

        Assert.Equal(1, response.TotalRows);
        Assert.Equal(1, response.ImportedRows);
        Assert.Equal(0, response.SkippedRows);
        Assert.Equal(1, response.Created.Courses);
        Assert.Equal(1, response.Created.Classes);
        Assert.Empty(response.Issues);

        var row = await RowAsync(db, id);

        Assert.Equal("checksum-abc", row.SourceChecksum);

        Assert.Equal(
            new DateTime(2026, 1, 5, 12, 0, 0).Ticks,
            row.SourceModifiedAt.Ticks
        );

        Assert.Equal(
            "ADS1 Algoritmer og datastrukturer",
            row.Name
        );

        Assert.Equal("IT-ADS1-A25", row.Nickname);
        Assert.Equal(5, row.ECTS);
        Assert.Equal(2, row.ExecutionElementCount);

        Assert.Equal(
            new DateOnly(2025, 9, 1),
            row.StartDate
        );

        Assert.Equal(
            new DateOnly(2026, 1, 31),
            row.EndDate
        );

        Assert.Equal(
            "Software Technology Engineering - VIA University College, Campus Horsens",
            row.ActivityOffering
        );

        Assert.Equal(
            PlanningEducationElement.Assessment.Internal,
            row.AssessmentType
        );

        Assert.False(row.CompletionAssessment);
        Assert.Equal("7-trin", row.GradingScale);
        Assert.False(row.CombinedWrittenOral);
        Assert.False(row.Oral);
        Assert.False(row.PracticalExam);
        Assert.False(row.Project);
        Assert.True(row.Written);
        Assert.Equal("", row.Prefix);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task Ekstern_IsMappedToExternal(DbKind kind)
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var id = Guid.NewGuid();

        var row = TestWorkbook.WithValue(
            TestWorkbook.ValidRow(id),
            "Bedømmelsestype",
            "Ekstern"
        );

        await ImportAsync(db, TestWorkbook.Build(row));

        Assert.Equal(
            PlanningEducationElement.Assessment.External,
            (await RowAsync(db, id)).AssessmentType
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NoAssessmentEvent_IsCreated_ForImportedRows(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        await ImportAsync(
            db,
            TestWorkbook.Build(TestWorkbook.ValidRow())
        );

        await using var context = db.NewContext();

        Assert.Equal(
            0,
            await context.AssessmentEvent.CountAsync()
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task EachRow_GetsACourseAndAClass_WithTheDerivedValues(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        await ImportAsync(
            db,
            TestWorkbook.Build(TestWorkbook.ValidRow())
        );

        var course = Assert.Single(await CoursesAsync(db));

        Assert.Equal("ADS1", course.SueCode);
        Assert.Equal("IT", course.Prefix);
        Assert.Equal("Algoritmer og datastrukturer", course.Name);
        Assert.Equal(5, course.ECTS);

        await using (var context = db.NewContext())
        {
            var element = await context.PlanningEducationElement
                .Include(x => x.Campus)
                .SingleAsync();

            Assert.Equal("Horsens", element.Campus.Name);
            Assert.Equal(course.Id, element.CourseId);
        }

        Assert.Equal(0, course.Semester);
        Assert.Equal(0, course.PriorityTier);

        var theClass = Assert.Single(await ClassesAsync(db));

        Assert.Equal("IT-ADS1-A25", theClass.ClassCode);
        Assert.Equal("IT-ADS1-A25", theClass.NickName);
        Assert.Equal(course.Id, theClass.CourseId);

        Assert.Equal(
            new DateTime(2025, 9, 1).Ticks,
            theClass.StartDate.Ticks
        );

        Assert.Equal(
            new DateTime(2026, 1, 31).Ticks,
            theClass.EndDate.Ticks
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task SeveralClassesOfTheSameCourse_ShareOneCourse(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var autumn = TestWorkbook.ValidRow(
            nickname: "IT-ADS1-A25"
        );

        var spring = TestWorkbook.ValidRow(
            nickname: "IT-ADS1-S26"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(autumn, spring)
        );

        Assert.Equal(1, response.Created.Courses);
        Assert.Equal(2, response.Created.Classes);

        var course = Assert.Single(await CoursesAsync(db));

        var classes = await ClassesAsync(db);

        Assert.Equal(2, classes.Count);

        Assert.All(
            classes,
            c => Assert.Equal(course.Id, c.CourseId)
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task SameCodeInAnotherProgramme_IsAnotherCourse(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var ts = TestWorkbook.ValidRow(
            nickname: "TS-DBS1-S26",
            name: "DBS1 Databasesystemer"
        );

        var it = TestWorkbook.ValidRow(
            nickname: "IT-DBS1-S26",
            name: "DBS1 Databasesystemer"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(ts, it)
        );

        Assert.Equal(2, response.Created.Courses);

        Assert.Equal(
            ["IT", "TS"],
            (await CoursesAsync(db)).Select(c => c.Prefix)
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task CourseName_DropsTheCode_AlsoForTheSommerVariant(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var row = TestWorkbook.ValidRow(
            nickname: "BY-BET1-A25-Sommer",
            name: "BET1-Sommer - Grundlæggende materialelære og betonkonstruktioner"
        );

        await ImportAsync(db, TestWorkbook.Build(row));

        var course = Assert.Single(await CoursesAsync(db));

        Assert.Equal("BET1", course.SueCode);

        Assert.Equal(
            "Grundlæggende materialelære og betonkonstruktioner",
            course.Name
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ReImport_CreatesNoNewCoursesOrClasses(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var rows = new[]
        {
            TestWorkbook.UniqueRow(),
            TestWorkbook.UniqueRow()
        };

        await ImportAsync(db, TestWorkbook.Build(rows));

        var second = await ImportAsync(
            db,
            TestWorkbook.Build(rows)
        );

        Assert.Equal(0, second.Created.Courses);
        Assert.Equal(0, second.Created.Classes);
        Assert.Equal(2, (await CoursesAsync(db)).Count);
        Assert.Equal(2, (await ClassesAsync(db)).Count);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ExistingCourse_IsNeverOverwrittenByTheImport(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var id = Guid.NewGuid();

        var nickname = "IT-ADS1-A25";

        await ImportAsync(
            db,
            TestWorkbook.Build(
                TestWorkbook.ValidRow(id, nickname)
            )
        );

        await using (var context = db.NewContext())
        {
            await context.Course.ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Name, "Edited by a planner")
                .SetProperty(c => c.ECTS, 99)
                .SetProperty(c => c.Semester, 3));
        }

        var changed = TestWorkbook.WithValue(
            TestWorkbook.ValidRow(id, nickname),
            "Kontrolsum for række",
            "new"
        );

        changed = TestWorkbook.WithValue(
            changed,
            "Navn",
            "ADS1 Something else"
        );

        changed = TestWorkbook.WithValue(
            changed,
            "ECTS",
            10
        );

        await ImportAsync(db, TestWorkbook.Build(changed));

        var course = Assert.Single(await CoursesAsync(db));

        Assert.Equal("Edited by a planner", course.Name);
        Assert.Equal(99, course.ECTS);
        Assert.Equal(3, course.Semester);

        Assert.Equal(
            "ADS1 Something else",
            (await RowAsync(db, id)).Name
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task CourseEditedByAPlanner_IsStillFoundThroughItsClass(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var id = Guid.NewGuid();

        await ImportAsync(
            db,
            TestWorkbook.Build(TestWorkbook.ValidRow(id))
        );

        await using (var context = db.NewContext())
        {
            await context.Course.ExecuteUpdateAsync(s => s
                .SetProperty(c => c.SueCode, "FIXED")
                .SetProperty(c => c.Name, "Corrected name"));
        }

        var changed = TestWorkbook.WithValue(
            TestWorkbook.ValidRow(id),
            "Kontrolsum for række",
            "new"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(changed)
        );

        Assert.Equal(0, response.Created.Courses);
        Assert.Equal(0, response.Created.Classes);

        var course = Assert.Single(await CoursesAsync(db));

        Assert.Equal("FIXED", course.SueCode);

        Assert.Single(await ClassesAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ExistingClass_GetsTheDatesFromTheFileAgain(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var row = TestWorkbook.ValidRow();

        await ImportAsync(db, TestWorkbook.Build(row));

        await using (var context = db.NewContext())
        {
            await context.Class.ExecuteUpdateAsync(s => s
                .SetProperty(
                    c => c.StartDate,
                    new DateTime(
                        2000,
                        1,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc
                    )
                ));
        }

        await ImportAsync(db, TestWorkbook.Build(row));

        var theClass = Assert.Single(await ClassesAsync(db));

        Assert.Equal(
            new DateTime(2025, 9, 1).Ticks,
            theClass.StartDate.Ticks
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task DatabaseFilledByAnEarlierImport_GetsItsCoursesAndClassesOnTheNextImport(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var row = TestWorkbook.ValidRow();

        await ImportAsync(db, TestWorkbook.Build(row));

        await using (var context = db.NewContext())
        {
            await context.Class.ExecuteDeleteAsync();
            await context.Course.ExecuteDeleteAsync();
        }

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(row)
        );

        Assert.Equal(1, response.Created.Courses);
        Assert.Equal(1, response.Created.Classes);
        Assert.Single(await CoursesAsync(db));
        Assert.Single(await ClassesAsync(db));
        Assert.Single(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task MissingCampus_StillImports_AsAWarning(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var row = TestWorkbook.WithValue(
            TestWorkbook.ValidRow(),
            "Aktivitetsudbud",
            "A programme with no campus in its name"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(row)
        );

        Assert.Equal(1, response.ImportedRows);
        Assert.Equal(0, response.SkippedRows);

        var issue = Assert.Single(response.Issues);

        Assert.Equal("warning", issue.Severity);
        Assert.Equal("Aktivitetsudbud", issue.Column);

        await using var context = db.NewContext();

        Assert.Equal(
            "Unknown",
            await context.PlanningEducationElement
                .Select(x => x.Campus.Name)
                .SingleAsync()
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NicknameWithoutAProgrammePrefix_IsSkipped_AndNothingIsSaved(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var row = TestWorkbook.WithValue(
            TestWorkbook.ValidRow(),
            "Kaldenavn",
            "1-ADS1-A25"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(row)
        );

        Assert.Equal(1, response.SkippedRows);

        Assert.Equal(
            "Kaldenavn",
            Assert.Single(response.Issues).Column
        );

        Assert.Empty(await RowsAsync(db));
        Assert.Empty(await CoursesAsync(db));
        Assert.Empty(await ClassesAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ImportingTheSameFileTwice_CreatesNothingTheSecondTime(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var rows = new[]
        {
            TestWorkbook.UniqueRow(),
            TestWorkbook.UniqueRow()
        };

        var first = await ImportAsync(
            db,
            TestWorkbook.Build(rows)
        );

        var second = await ImportAsync(
            db,
            TestWorkbook.Build(rows)
        );

        Assert.Equal(2, first.Created.Classes);
        Assert.Equal(0, second.Created.Classes);
        Assert.Equal(2, second.ImportedRows);
        Assert.Equal(2, (await RowsAsync(db)).Count);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task UnchangedChecksum_LeavesTheRowAlone(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var id = Guid.NewGuid();

        var row = TestWorkbook.ValidRow(id);

        await ImportAsync(db, TestWorkbook.Build(row));

        await using (var context = db.NewContext())
        {
            await context.PlanningEducationElement
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(
                        e => e.Name,
                        "edited in the database"
                    ));
        }

        await ImportAsync(db, TestWorkbook.Build(row));

        Assert.Equal(
            "edited in the database",
            (await RowAsync(db, id)).Name
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ChangedChecksum_UpdatesFileFields_ButKeepsTheHandFilledPrefix(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var id = Guid.NewGuid();

        await ImportAsync(
            db,
            TestWorkbook.Build(TestWorkbook.ValidRow(id))
        );

        await using (var context = db.NewContext())
        {
            await context.PlanningEducationElement
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Prefix, "IT"));
        }

        var changed = TestWorkbook.ValidRow(id);

        changed = TestWorkbook.WithValue(
            changed,
            "Kontrolsum for række",
            "checksum-NEW"
        );

        changed = TestWorkbook.WithValue(
            changed,
            "Navn",
            "ADS1 Renamed"
        );

        changed = TestWorkbook.WithValue(
            changed,
            "ECTS",
            10
        );

        changed = TestWorkbook.WithValue(
            changed,
            "Bedømmelsestype",
            "Ekstern"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(changed)
        );

        Assert.Equal(1, response.ImportedRows);
        Assert.Equal(0, response.Created.Classes);

        var row = await RowAsync(db, id);

        Assert.Equal("checksum-NEW", row.SourceChecksum);
        Assert.Equal("ADS1 Renamed", row.Name);
        Assert.Equal(10, row.ECTS);

        Assert.Equal(
            PlanningEducationElement.Assessment.External,
            row.AssessmentType
        );

        Assert.Equal("IT", row.Prefix);

        Assert.Single(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task MixOfNewChangedAndUnchanged_IsHandledRowByRow(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var unchangedId = Guid.NewGuid();

        var changedId = Guid.NewGuid();

        var unchanged = TestWorkbook.UniqueRow(unchangedId);

        var changedBefore = TestWorkbook.UniqueRow(changedId);

        await ImportAsync(
            db,
            TestWorkbook.Build(unchanged, changedBefore)
        );

        var changed = TestWorkbook.WithValue(
            changedBefore,
            "Kontrolsum for række",
            "new"
        );

        changed = TestWorkbook.WithValue(
            changed,
            "ECTS",
            10
        );

        var brandNew = TestWorkbook.UniqueRow();

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(
                unchanged,
                changed,
                brandNew
            )
        );

        Assert.Equal(3, response.TotalRows);
        Assert.Equal(3, response.ImportedRows);
        Assert.Equal(1, response.Created.Classes);
        Assert.Equal(3, (await RowsAsync(db)).Count);

        Assert.Equal(
            10,
            (await RowAsync(db, changedId)).ECTS
        );
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task BadRow_IsSkippedAndReported_OthersAreImported(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var bad = TestWorkbook.WithValue(
            TestWorkbook.UniqueRow(),
            "Startdato",
            "not a date"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(
                TestWorkbook.UniqueRow(),
                bad,
                TestWorkbook.UniqueRow()
            )
        );

        Assert.Equal(3, response.TotalRows);
        Assert.Equal(2, response.ImportedRows);
        Assert.Equal(1, response.SkippedRows);

        var issue = Assert.Single(response.Issues);

        Assert.Equal("error", issue.Severity);
        Assert.Equal("Data", issue.Sheet);
        Assert.Equal(3, issue.Row);
        Assert.Contains("Startdato", issue.Message);
        Assert.Equal(2, (await RowsAsync(db)).Count);
        Assert.Equal(2, (await CoursesAsync(db)).Count);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task FractionalEcts_IsSkipped_AndNamesTheColumn(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var row = TestWorkbook.WithValue(
            TestWorkbook.ValidRow(),
            "ECTS",
            7.5
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(row)
        );

        Assert.Equal(0, response.ImportedRows);
        Assert.Equal(1, response.SkippedRows);

        var issue = Assert.Single(response.Issues);

        Assert.Equal("ECTS", issue.Column);
        Assert.Equal(2, issue.Row);
        Assert.Empty(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task UnknownAssessmentType_IsSkipped_AndNamesTheColumn(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var row = TestWorkbook.WithValue(
            TestWorkbook.ValidRow(),
            "Bedømmelsestype",
            "Mixed"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(row)
        );

        Assert.Equal(1, response.SkippedRows);

        Assert.Equal(
            "Bedømmelsestype",
            Assert.Single(response.Issues).Column
        );

        Assert.Empty(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task FileWithOnlyBadRows_ImportsNothing_WithoutThrowing(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var bad1 = TestWorkbook.WithValue(
            TestWorkbook.UniqueRow(),
            "Mundtlig",
            "maybe"
        );

        var bad2 = TestWorkbook.WithValue(
            TestWorkbook.UniqueRow(),
            "Startdato",
            "x"
        );

        var response = await ImportAsync(
            db,
            TestWorkbook.Build(bad1, bad2)
        );

        Assert.Equal(2, response.TotalRows);
        Assert.Equal(0, response.ImportedRows);
        Assert.Equal(2, response.SkippedRows);
        Assert.Equal(2, response.Issues.Count);
        Assert.Empty(await RowsAsync(db));
        Assert.Empty(await CoursesAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task MissingColumn_ThrowsImportFileException_AndSavesNothing(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        var headers = TestWorkbook.Headers
            .Where(h => h != "Kaldenavn")
            .ToArray();

        var row = TestWorkbook.ValidRow()
            .Where((_, i) => i != 4)
            .ToArray();

        var error =
            await Assert.ThrowsAsync<ImportFileException>(
                () => ImportAsync(
                    db,
                    TestWorkbook.Build(headers, row)
                )
            );

        Assert.Contains("Kaldenavn", error.Message);
        Assert.Empty(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NotAnExcelFile_ThrowsImportFileException(
        DbKind kind
    )
    {
        await using var db =
            await TestDatabase.CreateAsync(kind);

        using var notExcel = new MemoryStream(
            "plain text, not an xlsx"u8.ToArray()
        );

        await Assert.ThrowsAsync<ImportFileException>(
            () => ImportAsync(db, notExcel)
        );

        Assert.Empty(await RowsAsync(db));
    }

    [Fact]
    public async Task FailureWhileSaving_RollsBackEarlierUpdates_AndNewCoursesAndClasses()
    {
        await using var db =
            await TestDatabase.CreateAsync(DbKind.Sqlite);

        var existingId = Guid.NewGuid();

        await ImportAsync(
            db,
            TestWorkbook.Build(
                TestWorkbook.ValidRow(existingId)
            )
        );

        await using (var context = db.NewContext())
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TRIGGER fail_on_boom BEFORE INSERT ON \"PlanningEducationElement\" " +
                "WHEN NEW.\"Name\" = 'boom' BEGIN SELECT RAISE(ABORT, 'boom'); END;"
            );
        }

        var changed = TestWorkbook.WithValue(
            TestWorkbook.ValidRow(existingId),
            "Kontrolsum for række",
            "new"
        );

        changed = TestWorkbook.WithValue(
            changed,
            "Navn",
            "ADS1 changed before the failure"
        );

        var poison = TestWorkbook.WithValue(
            TestWorkbook.UniqueRow(),
            "Navn",
            "boom"
        );

        await Assert.ThrowsAnyAsync<DbUpdateException>(
            () => ImportAsync(
                db,
                TestWorkbook.Build(changed, poison)
            )
        );

        var survivor = await RowAsync(db, existingId);

        Assert.Equal(
            "ADS1 Algoritmer og datastrukturer",
            survivor.Name
        );

        Assert.Equal(
            "checksum-abc",
            survivor.SourceChecksum
        );

        Assert.Single(await RowsAsync(db));
        Assert.Single(await CoursesAsync(db));
        Assert.Single(await ClassesAsync(db));
    }

    [RealExportFact]
    public async Task RealExport_IsImported_WithItsCoursesAndClasses_AndAReimportChangesNothing()
    {
        await using var db =
            await TestDatabase.CreateAsync(DbKind.Sqlite);

        await using (var file = new FileStream(
                         ExcelTest.Path,
                         FileMode.Open,
                         FileAccess.Read,
                         FileShare.ReadWrite
                     ))
        {
            var first = await ImportAsync(db, file);

            Assert.Equal(108, first.TotalRows);
            Assert.Equal(108, first.ImportedRows);
            Assert.Empty(first.Issues);
            Assert.Equal(98, first.Created.Courses);
            Assert.Equal(108, first.Created.Classes);
        }

        var rows = await RowsAsync(db);

        Assert.Equal(108, rows.Count);

        Assert.Equal(
            63,
            rows.Count(r =>
                r.AssessmentType ==
                PlanningEducationElement.Assessment.Internal
            )
        );

        Assert.Equal(
            45,
            rows.Count(r =>
                r.AssessmentType ==
                PlanningEducationElement.Assessment.External
            )
        );

        Assert.Equal(33, rows.Count(r => r.Written));
        Assert.Equal(54, rows.Count(r => r.Oral));

        var courses = await CoursesAsync(db);

        Assert.Equal(98, courses.Count);

        await using (var context = db.NewContext())
        {
            var offerings = await context.PlanningEducationElement
                .Include(x => x.Campus)
                .ToListAsync();

            Assert.All(
                offerings,
                x => Assert.StartsWith(
                    CourseDerivation.Campus(x.ActivityOffering),
                    x.Campus.Name
                )
            );
        }

        Assert.DoesNotContain(
            courses,
            c => c.Prefix.Length != 2 ||
                 c.SueCode.Length == 0 ||
                 c.Name.Length == 0
        );

        Assert.Contains(
            courses,
            c => c.Prefix == "IT" &&
                 c.SueCode == "ADS1" &&
                 c.Name == "Algoritmer og datastrukturer"
        );

        Assert.Equal(108, (await ClassesAsync(db)).Count);

        await using var again = new FileStream(
            ExcelTest. Path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );

        var second = await ImportAsync(db, again);

        Assert.Equal(0, second.Created.Courses);
        Assert.Equal(0, second.Created.Classes);
        Assert.Equal(108, (await RowsAsync(db)).Count);
        Assert.Equal(98, (await CoursesAsync(db)).Count);
    }
}