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
    public static IEnumerable<object[]> Databases() => TestDatabase.Kinds();

    // ---------- helpers ----------

    // Runs the service the way a request does: a fresh DbContext and an "uploaded" file.
    private static async Task<ImportResponse> ImportAsync(TestDatabase db, Stream workbook, string fileName = "export.xlsx")
    {
        await using var context = db.NewContext();
        var service = new ImportService(new ExcelImportParser(), context, NullLogger<ImportService>.Instance);
        return await service.ImportAsync(new FormFile(workbook, 0, workbook.Length, "file", fileName));
    }

    private static async Task<List<PlanningEducationElement>> RowsAsync(TestDatabase db)
    {
        await using var context = db.NewContext();
        return await context.PlanningEducationElement.AsNoTracking().OrderBy(e => e.Nickmane).ToListAsync();
    }

    private static async Task<PlanningEducationElement> RowAsync(TestDatabase db, Guid id)
    {
        await using var context = db.NewContext();
        return await context.PlanningEducationElement.AsNoTracking().SingleAsync(e => e.Id == id);
    }

    // ---------- inserting ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NewRow_IsInserted_WithAllFieldsMapped(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var id = Guid.NewGuid();

        var response = await ImportAsync(db, TestWorkbook.Build(TestWorkbook.ValidRow(id)));

        Assert.Equal(1, response.TotalRows);
        Assert.Equal(1, response.ImportedRows);
        Assert.Equal(0, response.SkippedRows);
        Assert.Equal(1, response.Created.Classes);
        Assert.Empty(response.Issues);

        var row = await RowAsync(db, id);
        Assert.Equal("checksum-abc", row.SourceChecksum);
        // Compared as wall-clock ticks: the file has no time zone, and SQLite does not keep the kind.
        Assert.Equal(new DateTime(2026, 1, 5, 12, 0, 0).Ticks, row.SourceModifiedAt.Ticks);
        Assert.Equal("ADS1 Algoritmer og datastrukturer", row.Name);
        Assert.Equal("IT-ADS1-A25", row.Nickmane);
        Assert.Equal(5, row.ECTS);
        Assert.Equal(2, row.ExecutionElementCount);
        Assert.Equal(new DateOnly(2025, 9, 1), row.StartDate);
        Assert.Equal(new DateOnly(2026, 1, 31), row.EndDate);
        Assert.Equal("Software Technology Engineering - Horsens", row.ActivityOffering);
        Assert.Equal(PlanningEducationElement.Assessment.Internal, row.AssessmenType);
        Assert.False(row.CompletionAssesment);
        Assert.Equal("7-trin", row.GradingScale);
        Assert.False(row.CombinedWrittenOral);
        Assert.False(row.Oral);
        Assert.False(row.PracticalExam);
        Assert.False(row.Project);
        Assert.True(row.Written);
        Assert.Equal("", row.Prefix); // not in the file: filled in by hand later
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task Ekstern_IsMappedToExternal(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var id = Guid.NewGuid();
        var row = TestWorkbook.WithValue(TestWorkbook.ValidRow(id), "Bedømmelsestype", "Ekstern");

        await ImportAsync(db, TestWorkbook.Build(row));

        Assert.Equal(PlanningEducationElement.Assessment.External, (await RowAsync(db, id)).AssessmenType);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NoAssessmentEvent_IsCreated_ForImportedRows(DbKind kind)
    {
        // Regression: the models used to create each other endlessly (stack overflow), and an element
        // does not need an event yet. Importing must not create or require one.
        await using var db = await TestDatabase.CreateAsync(kind);

        await ImportAsync(db, TestWorkbook.Build(TestWorkbook.ValidRow()));

        await using var context = db.NewContext();
        Assert.Equal(0, await context.AssessmentEvent.CountAsync());
    }

    // ---------- re-importing ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ImportingTheSameFileTwice_CreatesNothingTheSecondTime(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var rows = new[] { TestWorkbook.ValidRow(), TestWorkbook.ValidRow() };

        var first = await ImportAsync(db, TestWorkbook.Build(rows));
        var second = await ImportAsync(db, TestWorkbook.Build(rows));

        Assert.Equal(2, first.Created.Classes);
        Assert.Equal(0, second.Created.Classes);
        Assert.Equal(2, second.ImportedRows);
        Assert.Equal(2, (await RowsAsync(db)).Count);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task UnchangedChecksum_LeavesTheRowAlone(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var id = Guid.NewGuid();
        var row = TestWorkbook.ValidRow(id);
        await ImportAsync(db, TestWorkbook.Build(row));

        // Change the row in the database only. The file (same checksum) still says the old name.
        await using (var context = db.NewContext())
        {
            await context.PlanningEducationElement.Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Name, "edited in the database"));
        }

        await ImportAsync(db, TestWorkbook.Build(row));

        // The checksum says "nothing changed at the source", so the row must not be rewritten.
        Assert.Equal("edited in the database", (await RowAsync(db, id)).Name);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task ChangedChecksum_UpdatesFileFields_ButKeepsTheHandFilledPrefix(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var id = Guid.NewGuid();
        await ImportAsync(db, TestWorkbook.Build(TestWorkbook.ValidRow(id)));

        // A planner fills in the prefix by hand.
        await using (var context = db.NewContext())
        {
            await context.PlanningEducationElement.Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Prefix, "IT"));
        }

        // The source system changes the row: new checksum, new name, new ECTS, new assessment type.
        var changed = TestWorkbook.ValidRow(id);
        changed = TestWorkbook.WithValue(changed, "Kontrolsum for række", "checksum-NEW");
        changed = TestWorkbook.WithValue(changed, "Navn", "ADS1 Renamed");
        changed = TestWorkbook.WithValue(changed, "ECTS", 10);
        changed = TestWorkbook.WithValue(changed, "Bedømmelsestype", "Ekstern");
        var response = await ImportAsync(db, TestWorkbook.Build(changed));

        Assert.Equal(1, response.ImportedRows);
        Assert.Equal(0, response.Created.Classes); // updated, not created

        var row = await RowAsync(db, id);
        Assert.Equal("checksum-NEW", row.SourceChecksum);
        Assert.Equal("ADS1 Renamed", row.Name);
        Assert.Equal(10, row.ECTS);
        Assert.Equal(PlanningEducationElement.Assessment.External, row.AssessmenType);
        Assert.Equal("IT", row.Prefix); // the import must never overwrite what people typed in
        Assert.Single(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task MixOfNewChangedAndUnchanged_IsHandledRowByRow(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var unchangedId = Guid.NewGuid();
        var changedId = Guid.NewGuid();
        var unchanged = TestWorkbook.ValidRow(unchangedId);
        await ImportAsync(db, TestWorkbook.Build(unchanged, TestWorkbook.ValidRow(changedId)));

        var changed = TestWorkbook.WithValue(TestWorkbook.ValidRow(changedId), "Kontrolsum for række", "new");
        changed = TestWorkbook.WithValue(changed, "Navn", "Changed name");
        var brandNew = TestWorkbook.ValidRow();
        var response = await ImportAsync(db, TestWorkbook.Build(unchanged, changed, brandNew));

        Assert.Equal(3, response.TotalRows);
        Assert.Equal(3, response.ImportedRows);
        Assert.Equal(1, response.Created.Classes); // only the brand-new row counts as created
        Assert.Equal(3, (await RowsAsync(db)).Count);
        Assert.Equal("Changed name", (await RowAsync(db, changedId)).Name);
    }

    // ---------- bad rows: skipped and reported, the rest is still imported ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task BadRow_IsSkippedAndReported_OthersAreImported(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Startdato", "not a date");

        var response = await ImportAsync(db, TestWorkbook.Build(TestWorkbook.ValidRow(), bad, TestWorkbook.ValidRow()));

        Assert.Equal(3, response.TotalRows);
        Assert.Equal(2, response.ImportedRows);
        Assert.Equal(1, response.SkippedRows);
        var issue = Assert.Single(response.Issues);
        Assert.Equal("error", issue.Severity);   // the website filters on this exact text
        Assert.Equal("Data", issue.Sheet);       // the sheet name of the generated test workbook
        Assert.Equal(3, issue.Row);              // header is row 1, so the second data row is row 3
        Assert.Contains("Startdato", issue.Message);
        Assert.Equal(2, (await RowsAsync(db)).Count);
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task FractionalEcts_IsSkipped_AndNamesTheColumn(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var row = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "ECTS", 7.5);

        var response = await ImportAsync(db, TestWorkbook.Build(row));

        Assert.Equal(0, response.ImportedRows);
        Assert.Equal(1, response.SkippedRows);
        var issue = Assert.Single(response.Issues);
        Assert.Equal("ECTS", issue.Column);
        Assert.Equal(2, issue.Row);
        Assert.Empty(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task UnknownAssessmentType_IsSkipped_AndNamesTheColumn(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var row = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Bedømmelsestype", "Mixed");

        var response = await ImportAsync(db, TestWorkbook.Build(row));

        Assert.Equal(1, response.SkippedRows);
        Assert.Equal("Bedømmelsestype", Assert.Single(response.Issues).Column);
        Assert.Empty(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task FileWithOnlyBadRows_ImportsNothing_WithoutThrowing(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var bad1 = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Mundtlig", "maybe");
        var bad2 = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Startdato", "x");

        var response = await ImportAsync(db, TestWorkbook.Build(bad1, bad2));

        Assert.Equal(2, response.TotalRows);
        Assert.Equal(0, response.ImportedRows);
        Assert.Equal(2, response.SkippedRows);
        Assert.Equal(2, response.Issues.Count);
        Assert.Empty(await RowsAsync(db));
    }

    // ---------- whole-file problems: nothing can be imported ----------

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task MissingColumn_ThrowsImportFileException_AndSavesNothing(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        var headers = TestWorkbook.Headers.Where(h => h != "Kaldenavn").ToArray();
        var row = TestWorkbook.ValidRow().Where((_, i) => i != 4).ToArray(); // Kaldenavn is column 5

        var error = await Assert.ThrowsAsync<ImportFileException>(
            () => ImportAsync(db, TestWorkbook.Build(headers, row)));

        Assert.Contains("Kaldenavn", error.Message);
        Assert.Empty(await RowsAsync(db));
    }

    [Theory]
    [MemberData(nameof(Databases))]
    public async Task NotAnExcelFile_ThrowsImportFileException(DbKind kind)
    {
        await using var db = await TestDatabase.CreateAsync(kind);
        using var notExcel = new MemoryStream("plain text, not an xlsx"u8.ToArray());

        await Assert.ThrowsAsync<ImportFileException>(() => ImportAsync(db, notExcel));

        Assert.Empty(await RowsAsync(db));
    }

    // ---------- all-or-nothing ----------

    [Fact]
    public async Task FailureWhileSaving_RollsBackEarlierUpdates()
    {
        // The service updates existing rows first and inserts new ones last. If the insert fails, the
        // update that already ran must be undone, otherwise a crash leaves half an import behind.
        // SQLite only: the failure is forced with a trigger, which is written differently in PostgreSQL.
        await using var db = await TestDatabase.CreateAsync(DbKind.Sqlite);
        var existingId = Guid.NewGuid();
        await ImportAsync(db, TestWorkbook.Build(TestWorkbook.ValidRow(existingId)));

        await using (var context = db.NewContext())
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TRIGGER fail_on_boom BEFORE INSERT ON \"PlanningEducationElement\" " +
                "WHEN NEW.\"Name\" = 'boom' BEGIN SELECT RAISE(ABORT, 'boom'); END;");
        }

        var changed = TestWorkbook.WithValue(TestWorkbook.ValidRow(existingId), "Kontrolsum for række", "new");
        changed = TestWorkbook.WithValue(changed, "Navn", "changed before the failure");
        var poison = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Navn", "boom");

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ImportAsync(db, TestWorkbook.Build(changed, poison)));

        var survivor = await RowAsync(db, existingId);
        Assert.Equal("ADS1 Algoritmer og datastrukturer", survivor.Name); // the update was rolled back
        Assert.Equal("checksum-abc", survivor.SourceChecksum);
        Assert.Single(await RowsAsync(db));
    }

    // ---------- the real export ----------

    [RealExportFact]
    public async Task RealExport_IsImported_AndAReimportChangesNothing()
    {
        await using var db = await TestDatabase.CreateAsync(DbKind.Sqlite);

        await using (var file = new FileStream(ExcelTest.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            var first = await ImportAsync(db, file);

            Assert.Equal(108, first.TotalRows);
            Assert.Equal(108, first.ImportedRows);
            Assert.Equal(108, first.Created.Classes);
            Assert.Empty(first.Issues);
        }

        var rows = await RowsAsync(db);
        Assert.Equal(108, rows.Count);
        Assert.Equal(63, rows.Count(r => r.AssessmenType == PlanningEducationElement.Assessment.Internal));
        Assert.Equal(45, rows.Count(r => r.AssessmenType == PlanningEducationElement.Assessment.External));
        Assert.Equal(33, rows.Count(r => r.Written));
        Assert.Equal(54, rows.Count(r => r.Oral));

        await using var again = new FileStream(ExcelTest.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var second = await ImportAsync(db, again);
        Assert.Equal(0, second.Created.Classes);
        Assert.Equal(108, (await RowsAsync(db)).Count);
    }
}
