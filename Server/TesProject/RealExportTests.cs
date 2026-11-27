using ExamPlannerServer.Import;

namespace TesProject;

// A [Fact] that skips itself when the real export is not on this machine,
// so teammates (and CI) do not get failing tests because of a missing file.
public sealed class RealExportFactAttribute : FactAttribute
{
    public RealExportFactAttribute()
    {
        if (!File.Exists(ExcelTest.path))
            Skip = $"Real export not found at {ExcelTest.path}";
    }
}

public class RealExportTests
{
    // FileShare.ReadWrite lets us read the file even while it is open in Excel
    // (Excel locks it; File.OpenRead would throw "being used by another process").
    private static FileStream OpenExport() =>
        new(ExcelTest.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    [RealExportFact]
    public void RealExport_IsParsedWithoutErrors()
    {
        using var file = OpenExport();

        var result = new ExcelImportParser().Parse(file);

        Assert.Empty(result.Errors);
        Assert.Equal(108, result.Rows.Count);
    }

    [RealExportFact]
    public void RealExport_FirstRow_HasExpectedValues()
    {
        using var file = OpenExport();

        var first = new ExcelImportParser().Parse(file).Rows.First();

        Assert.Equal(2, first.SourceRowNumber);
        Assert.Equal(Guid.Parse("cc54bd85-a622-ef11-a361-005056b27d6c"), first.PlanningElementId);
        Assert.Equal("ADS1 Algoritmer og datastrukturer", first.Name);
        Assert.Equal("IT-ADS1-A25", first.Nickname);
        Assert.Equal(5m, first.Ects);
        Assert.Equal(2, first.ExecutionElementCount);
        Assert.Equal(new DateTime(2025, 9, 1), first.StartDate);
        Assert.Equal("Intern", first.AssessmentType);
        Assert.Equal("7-trin", first.GradingScale);
        Assert.True(first.Written);
        Assert.False(first.Oral);
    }

    [RealExportFact]
    public void RealExport_IdsAreUnique()
    {
        using var file = OpenExport();

        var rows = new ExcelImportParser().Parse(file).Rows;

        Assert.Equal(rows.Count, rows.Select(r => r.PlanningElementId).Distinct().Count());
    }
}
