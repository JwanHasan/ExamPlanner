using ClosedXML.Excel;
namespace TesProject;

public class ExcelTest
{
        // public so RealExportTests can reuse it
        public const string path = @"C:\Users\lsuga\Downloads\Visning til Avanceret søgning efter Planlægningsuddannelseselement 30-09-2026 07-51-19.xlsx";

    [Fact]
    public void Step_A_OpenWorkbook()
    {
        using var wb = new XLWorkbook(path);

        foreach (var s in wb.Worksheets)
        {
            Console.WriteLine($"{s.Name} | {s.Visibility} | tables: {s.Tables.Count()}");
        }
            Assert.Equal(2, wb.Worksheets.Count);

    }

    [Fact]
    public void Step_B_OpenWorkbook() //check headers and row count
    {
        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheet("Raw data from system");
        var table = ws.Tables.First();

        var headers = table.HeadersRow().Cells().Select(c => c.GetString()).ToList();

        Assert.Equal(18, headers.Count);
        Assert.Contains("Kaldenavn", headers);
        Assert.Equal(108, table.DataRange.RowCount());
    }

    [Fact]
    public void StartDate_IsReadAsDateTime()
    {
        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheet("Raw data from system");
        var table = ws.Tables.First();
        var row = table.DataRange.Rows().First();
        var cell = row.Field("Startdato");

        Console.WriteLine(cell.DataType);
        Console.WriteLine(cell.GetString());
        Console.WriteLine(cell.GetDateTime());

        // these two lines were outside the method in your version (after its closing brace)
        Assert.Equal(XLDataType.DateTime, cell.DataType);
        Assert.Equal(new DateTime(2025, 9, 1), cell.GetDateTime());
    }
}
