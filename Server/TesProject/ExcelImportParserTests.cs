using ExamPlannerServer.Import;

namespace TesProject;

public class ExcelImportParserTests
{
    private static ParseResult Parse(Stream stream) => new ExcelImportParser().Parse(stream);

    [Fact]
    public void ValidRow_IsMappedToAllFields()
    {
        var id = Guid.NewGuid();
        using var file = TestWorkbook.Build(TestWorkbook.ValidRow(id));

        var result = Parse(file);

        Assert.Empty(result.Errors);
        var row = Assert.Single(result.Rows);
        Assert.Equal(2, row.SourceRowNumber);   // header is row 1, first data row is row 2
        Assert.Equal(id, row.PlanningElementId);
        Assert.Equal("checksum-abc", row.SourceChecksum);
        Assert.Equal(new DateTime(2026, 1, 5, 12, 0, 0), row.SourceModifiedAt);
        Assert.Equal("ADS1 Algoritmer og datastrukturer", row.Name);
        Assert.Equal("IT-ADS1-A25", row.Nickname);
        Assert.Equal(5m, row.Ects);
        Assert.Equal(2, row.ExecutionElementCount);
        Assert.Equal(new DateTime(2025, 9, 1), row.StartDate);
        Assert.Equal(new DateTime(2026, 1, 31), row.EndDate);
        Assert.Equal("Software Technology Engineering - Horsens", row.ActivityOffering);
        Assert.Equal("Intern", row.AssessmentType);
        Assert.False(row.CompletionAssessment);
        Assert.Equal("7-trin", row.GradingScale);
        Assert.False(row.CombinedWrittenOral);
        Assert.False(row.Oral);
        Assert.False(row.PracticalExam);
        Assert.False(row.Project);
        Assert.True(row.Written);
    }

    [Fact]
    public void Columns_AreFoundByNameNotPosition()
    {
        // Same data, columns in reverse order.
        var reversedHeaders = TestWorkbook.Headers.Reverse().ToArray();
        var reversedRow = TestWorkbook.ValidRow().Reverse().ToArray();
        using var file = TestWorkbook.Build(reversedHeaders, reversedRow);

        var result = Parse(file);

        Assert.Empty(result.Errors);
        var row = Assert.Single(result.Rows);
        Assert.Equal("IT-ADS1-A25", row.Nickname);
        Assert.True(row.Written);
    }

    [Fact]
    public void HeaderMatching_IgnoresCaseAndSurroundingSpaces()
    {
        var headers = (string[])TestWorkbook.Headers.Clone();
        headers[3] = "  navn ";   // was "Navn"
        using var file = TestWorkbook.Build(headers, TestWorkbook.ValidRow());

        var result = Parse(file);

        Assert.Empty(result.Errors);
        Assert.Single(result.Rows);
    }

    [Fact]
    public void ExtraColumns_AreIgnored()
    {
        var headers = TestWorkbook.Headers.Append("Something new").ToArray();
        var row = TestWorkbook.ValidRow().Append("x").ToArray();
        using var file = TestWorkbook.Build(headers, row);

        var result = Parse(file);

        Assert.Empty(result.Errors);
        Assert.Single(result.Rows);
    }

    [Fact]
    public void MissingColumn_IsReportedByName_AndNoRowsAreReturned()
    {
        var headers = TestWorkbook.Headers.Where(h => h != "Kaldenavn").ToArray();
        var row = TestWorkbook.ValidRow().Where((_, i) => i != 4).ToArray(); // Kaldenavn is column 5
        using var file = TestWorkbook.Build(headers, row);

        var result = Parse(file);

        Assert.Empty(result.Rows);
        var error = Assert.Single(result.Errors);
        Assert.Contains("Kaldenavn", error);
    }

    [Fact]
    public void WorkbookWithoutTable_ReturnsError()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        workbook.AddWorksheet("Data").Cell(1, 1).Value = "just a cell, no table";
        using var file = new MemoryStream();
        workbook.SaveAs(file);
        file.Position = 0;

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void NotAnExcelFile_ReturnsError_InsteadOfThrowing()
    {
        using var file = new MemoryStream("this is plain text, not an xlsx"u8.ToArray());

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void BadDate_SkipsOnlyThatRow_AndNamesTheRowNumber()
    {
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Startdato", "not a date");
        using var file = TestWorkbook.Build(TestWorkbook.ValidRow(), bad, TestWorkbook.ValidRow());

        var result = Parse(file);

        Assert.Equal(2, result.Rows.Count);        // rows 2 and 4 imported
        var error = Assert.Single(result.Errors);
        Assert.StartsWith("Row 3:", error);        // the bad one is sheet row 3
        Assert.Contains("Startdato", error);
    }

    [Fact]
    public void InvalidYesNo_SkipsRow()
    {
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Mundtlig", "maybe");
        using var file = TestWorkbook.Build(bad);

        var result = Parse(file);

        Assert.Empty(result.Rows);
        var error = Assert.Single(result.Errors);
        Assert.Contains("Mundtlig", error);
    }

    [Fact]
    public void YesNo_IsCaseInsensitive()
    {
        var row = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Mundtlig", " ja ");
        using var file = TestWorkbook.Build(row);

        var result = Parse(file);

        Assert.True(Assert.Single(result.Rows).Oral);
    }

    [Fact]
    public void MissingText_SkipsRow()
    {
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Kaldenavn", null);
        using var file = TestWorkbook.Build(bad);

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Contains("Kaldenavn", Assert.Single(result.Errors));
    }

    [Fact]
    public void AllProblemsOfARow_AreReportedTogether()
    {
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Startdato", "x");
        bad = TestWorkbook.WithValue(bad, "Skriftlig", "y");
        using var file = TestWorkbook.Build(bad);

        var error = Assert.Single(Parse(file).Errors);

        Assert.Contains("Startdato", error);
        Assert.Contains("Skriftlig", error);
    }

    [Fact]
    public void EndDateBeforeStartDate_SkipsRow()
    {
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Slutdato", new DateTime(2025, 1, 1));
        using var file = TestWorkbook.Build(bad);

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Contains("Slutdato", Assert.Single(result.Errors));
    }

    [Fact]
    public void DuplicateId_SkipsTheSecondOccurrence()
    {
        var id = Guid.NewGuid();
        using var file = TestWorkbook.Build(TestWorkbook.ValidRow(id), TestWorkbook.ValidRow(id));

        var result = Parse(file);

        Assert.Single(result.Rows);
        var error = Assert.Single(result.Errors);
        Assert.StartsWith("Row 3:", error);
    }

    [Fact]
    public void InvalidId_SkipsRow()
    {
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "Planlægningsuddannelseselement", "not-a-guid");
        using var file = TestWorkbook.Build(bad);

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void BlankRowInsideTable_IsIgnoredWithoutError()
    {
        var blank = new object?[TestWorkbook.Headers.Length];
        using var file = TestWorkbook.Build(TestWorkbook.ValidRow(), blank, TestWorkbook.ValidRow());

        var result = Parse(file);

        Assert.Empty(result.Errors);
        Assert.Equal(2, result.Rows.Count);
    }

    [Fact]
    public void Ects_KeepsDecimals()
    {
        var row = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "ECTS", 7.5);
        using var file = TestWorkbook.Build(row);

        Assert.Equal(7.5m, Assert.Single(Parse(file).Rows).Ects);
    }

    [Fact]
    public void Ects_AsText_SkipsRow()
    {
        var bad = TestWorkbook.WithValue(TestWorkbook.ValidRow(), "ECTS", "five");
        using var file = TestWorkbook.Build(bad);

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Contains("ECTS", Assert.Single(result.Errors));
    }
}
