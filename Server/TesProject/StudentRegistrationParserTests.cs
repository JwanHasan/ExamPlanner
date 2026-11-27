using ExamPlannerServer.Import;

namespace TesProject;

// The Opslagsark parser on its own: a workbook goes in, typed rows and problems come out. No database.
public class StudentRegistrationParserTests
{
    private static StudentRegistrationParseResult Parse(Stream stream) => new StudentRegistrationParser().Parse(stream);

    [Fact]
    public void ValidRow_IsMappedToAllFields()
    {
        var id = Guid.NewGuid();
        using var file = StudentWorkbook.Build(StudentWorkbook.ValidRow(id, student: "STU-0042", attempts: 2));

        var result = Parse(file);

        Assert.Empty(result.Issues);
        var row = Assert.Single(result.Rows);
        Assert.Equal(2, row.SourceRowNumber);       // header is row 1, first data row is row 2
        Assert.Equal(id, row.RegistrationId);
        Assert.Equal("STU-0042", row.StudentCode);
        Assert.Equal(42, row.StudentNumber);        // the number at the end of the id
        Assert.Equal("KA61-BDL-S26", row.PlanningElementNickname);
        Assert.Equal("BDLV6 Bæredygtig ledelse", row.PlanningElementName);
        Assert.Equal("KA61-BDL-F26", row.Hold);
        Assert.Equal(2, row.AttemptNo);
        Assert.Equal("Data", result.SheetName);
    }

    [Theory]
    [InlineData("STU-0001", 1)]
    [InlineData("STU-1154", 1154)]
    [InlineData("S2-17", 17)]          // digits earlier in the id do not count, only the number at the end
    [InlineData("123", 123)]
    public void StudentNumber_IsTheNumberAtTheEndOfTheId(string studentCode, int expected)
    {
        using var file = StudentWorkbook.Build(StudentWorkbook.ValidRow(student: studentCode));

        var result = Parse(file);

        Assert.Empty(result.Issues);
        Assert.Equal(expected, Assert.Single(result.Rows).StudentNumber);
    }

    [Fact]
    public void Headers_WithARepeatedBracket_AreFound()
    {
        // The real file calls the column "Kaldenavn (Planlægningsuddannelseselement) (Planlægningsuddannelseselement)".
        Assert.Contains(StudentWorkbook.Headers, h => h.EndsWith("(Planlægningsuddannelseselement) (Planlægningsuddannelseselement)"));
        using var file = StudentWorkbook.Build(StudentWorkbook.ValidRow());

        var result = Parse(file);

        Assert.Empty(result.Issues);
        Assert.Single(result.Rows);
    }

    [Fact]
    public void Columns_AreFoundByNameNotPosition()
    {
        using var file = TestWorkbook.Build(
            StudentWorkbook.Headers.Reverse().ToArray(),
            StudentWorkbook.ValidRow(student: "STU-0007").Reverse().ToArray());

        var result = Parse(file);

        Assert.Empty(result.Issues);
        Assert.Equal(7, Assert.Single(result.Rows).StudentNumber);
    }

    [Fact]
    public void ExtraAndBlankRows_AreIgnored()
    {
        var headers = StudentWorkbook.Headers.Append("Something new").ToArray();
        var withExtra = StudentWorkbook.ValidRow().Append("x").ToArray();
        var blank = new object?[headers.Length];
        using var file = TestWorkbook.Build(headers, withExtra, blank, StudentWorkbook.UniqueRow().Append("y").ToArray());

        var result = Parse(file);

        Assert.Empty(result.Issues);
        Assert.Equal(2, result.Rows.Count);
    }

    [Fact]
    public void MissingColumn_IsReportedByName_AndNoRowsAreReturned()
    {
        var index = Array.FindIndex(StudentWorkbook.Headers, h => h == "Hold");
        var headers = StudentWorkbook.Headers.Where((_, i) => i != index).ToArray();
        var row = StudentWorkbook.ValidRow().Where((_, i) => i != index).ToArray();
        using var file = TestWorkbook.Build(headers, row);

        var result = Parse(file);

        Assert.Empty(result.Rows);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(0, issue.Row);                   // a problem with the whole file
        Assert.Contains("Hold", issue.Message);
    }

    [Fact]
    public void ThePlanningElementExport_IsNotMistakenForAnOpslagsark()
    {
        // The other kind of file: it has no student column, so it must be rejected with the missing columns named.
        using var file = TestWorkbook.Build(TestWorkbook.ValidRow());

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Contains("Anonymt Student-ID", Assert.Single(result.Issues).Message);
    }

    [Fact]
    public void NotAnExcelFile_ReturnsAnError_InsteadOfThrowing()
    {
        using var file = new MemoryStream("plain text, not an xlsx"u8.ToArray());

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Equal(0, Assert.Single(result.Issues).Row);
    }

    [Fact]
    public void BadRow_IsSkippedWithItsRowNumber_OthersAreKept()
    {
        var bad = StudentWorkbook.WithValue(StudentWorkbook.UniqueRow(), StudentWorkbook.Attempts, "many");
        using var file = StudentWorkbook.Build(StudentWorkbook.UniqueRow(), bad, StudentWorkbook.UniqueRow());

        var result = Parse(file);

        Assert.Equal(2, result.Rows.Count);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(3, issue.Row);
        Assert.Contains("Antal forsøg", issue.Message);
    }

    [Theory]
    [InlineData("Gennemførelsesuddannelseselement", "not-a-guid")]
    [InlineData("Anonymt Student-ID", "")]
    [InlineData("Anonymt Student-ID", "no digits here")]
    [InlineData("Kaldenavn (Planlægningsuddannelseselement)", "")]
    [InlineData("Hold", "   ")]
    [InlineData("Antal forsøg", -1)]
    [InlineData("Antal forsøg", 1.5)]
    public void InvalidValues_SkipTheRow(string header, object value)
    {
        var bad = StudentWorkbook.WithValue(StudentWorkbook.UniqueRow(), header, value);
        using var file = StudentWorkbook.Build(bad);

        var result = Parse(file);

        Assert.Empty(result.Rows);
        Assert.Contains(header.Split(' ')[0], Assert.Single(result.Issues).Message);
    }

    [Fact]
    public void AllProblemsOfARow_AreReportedTogether()
    {
        var bad = StudentWorkbook.WithValue(StudentWorkbook.UniqueRow(), StudentWorkbook.Hold, "");
        bad = StudentWorkbook.WithValue(bad, StudentWorkbook.Attempts, "x");
        using var file = StudentWorkbook.Build(bad);

        var message = Assert.Single(Parse(file).Issues).Message;

        Assert.Contains("Hold", message);
        Assert.Contains("Antal forsøg", message);
    }

    [Fact]
    public void SameStudentOnTheSameElementTwice_SkipsTheSecondRow()
    {
        var first = StudentWorkbook.ValidRow(student: "STU-0005", nickname: "KA61-BDL-S26");
        var second = StudentWorkbook.ValidRow(student: "STU-0005", nickname: "KA61-BDL-S26", hold: "other");
        using var file = StudentWorkbook.Build(first, second);

        var result = Parse(file);

        Assert.Single(result.Rows);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(3, issue.Row);
        Assert.Contains("more than once", issue.Message);
    }

    [Fact]
    public void SameStudentOnDifferentElements_IsFine()
    {
        using var file = StudentWorkbook.Build(
            StudentWorkbook.ValidRow(student: "STU-0005", nickname: "KA61-BDL-S26"),
            StudentWorkbook.ValidRow(student: "STU-0005", nickname: "KA61-CIE-S26"));

        Assert.Equal(2, Parse(file).Rows.Count);
    }

    [Fact]
    public void SameRegistrationIdTwice_SkipsTheSecondRow()
    {
        var id = Guid.NewGuid();
        using var file = StudentWorkbook.Build(StudentWorkbook.UniqueRow(id), StudentWorkbook.UniqueRow(id));

        var result = Parse(file);

        Assert.Single(result.Rows);
        Assert.Contains("more than once", Assert.Single(result.Issues).Message);
    }

    // ---------- the real Opslagsark ----------

    [RealOpslagsarkFact]
    public void RealOpslagsark_IsParsedWithoutProblems()
    {
        using var file = new FileStream(OpslagsarkFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var result = Parse(file);

        Assert.Empty(result.Issues);
        Assert.Equal(6411, result.Rows.Count);
        Assert.Equal("Data fra esas", result.SheetName);   // the lookup sheet is not mistaken for the data
        // Worked out from the file independently of the code:
        Assert.Equal(1154, result.Rows.Select(r => r.StudentCode).Distinct().Count());
        Assert.Equal(370, result.Rows.Select(r => r.PlanningElementNickname).Distinct().Count());
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, result.Rows.Select(r => r.AttemptNo).Distinct().Order().ToArray());
    }

    [RealOpslagsarkFact]
    public void RealOpslagsark_FirstRow_HasTheExpectedValues()
    {
        using var file = new FileStream(OpslagsarkFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var first = Parse(file).Rows.First();

        Assert.Equal(2, first.SourceRowNumber);
        Assert.Equal(Guid.Parse("8bfdf618-c1d1-f011-a3c1-005056b2f3ea"), first.RegistrationId);
        Assert.Equal("STU-0001", first.StudentCode);
        Assert.Equal(1, first.StudentNumber);
        Assert.Equal("KA61-BDL-S26", first.PlanningElementNickname);
        Assert.StartsWith("BDLV6", first.PlanningElementName);
        Assert.Equal("KA61-BDL-F26", first.Hold);
        Assert.Equal(0, first.AttemptNo);
    }

    [RealOpslagsarkFact]
    public void RealOpslagsark_StudentsWithDifferentAttemptsOnDifferentRows_AreInTheData()
    {
        // The reason the importer has to decide what a student's AttemptNo is: 238 students of 1,154 differ between rows.
        using var file = new FileStream(OpslagsarkFile.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var rows = Parse(file).Rows;

        var differing = rows.GroupBy(r => r.StudentCode).Count(g => g.Select(r => r.AttemptNo).Distinct().Count() > 1);
        Assert.Equal(238, differing);
    }
}
