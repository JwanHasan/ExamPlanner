using ClosedXML.Excel;

namespace TesProject;

public class ExcelTest
{
    public static readonly string Path =
        Environment.GetEnvironmentVariable(
            "EXAM_PLANNER_TEST_EXPORT"
        )
        ?? System.IO.Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile
            ),
            "Downloads",
            "Visning til Avanceret søgning efter Planlægningsuddannelseselement 30-09-2026 07-51-19.xlsx"
        );

    [RealExportFact]
    public void Step_A_OpenWorkbook()
    {
        using var wb = new XLWorkbook(Path);

        Assert.Equal(2, wb.Worksheets.Count);
    }

    [RealExportFact]
    public void Step_B_OpenWorkbook()
    {
        using var wb = new XLWorkbook(Path);

        var ws = wb.Worksheet("Raw data from system");

        var table = ws.Tables.First();

        var headers = table.HeadersRow()
            .Cells()
            .Select(c => c.GetString())
            .ToList();

        Assert.Equal(18, headers.Count);

        Assert.Contains("Kaldenavn", headers);

        Assert.Equal(108, table.DataRange.RowCount());
    }

    [RealExportFact]
    public void StartDate_IsReadAsDateTime()
    {
        using var wb = new XLWorkbook(Path);

        var ws = wb.Worksheet("Raw data from system");

        var table = ws.Tables.First();

        var row = table.DataRange.Rows().First();

        var cell = row.Field("Startdato");

        Assert.Equal(XLDataType.DateTime, cell.DataType);

        Assert.Equal(
            new DateTime(2025, 9, 1),
            cell.GetDateTime()
        );
    }
}