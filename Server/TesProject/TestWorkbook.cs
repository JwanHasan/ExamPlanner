using ClosedXML.Excel;

namespace TesProject;

// Builds small .xlsx files in memory so parser tests do not need real (institutional) data
// and run on any machine. The layout copies the real Dynamics export: one sheet, one Excel Table.
internal static class TestWorkbook
{
    public static readonly string[] Headers =
    [
        "(Skal ikke ændres) Planlægningsuddannelseselement",
        "(Skal ikke ændres) Kontrolsum for række",
        "(Skal ikke ændres) Ændret",
        "Navn",
        "Kaldenavn",
        "ECTS",
        "Antal gennemførelsesuddannelseselementer",
        "Startdato",
        "Slutdato",
        "Aktivitetsudbud",
        "Bedømmelsestype",
        "Gennemførelse",
        "Karakterskala",
        "Kombineret skriftlig og mundtlig",
        "Mundtlig",
        "Praktisk prøve",
        "Projekt",
        "Skriftlig"
    ];

    // One valid data row, values in the same order as Headers.
    public static object?[] ValidRow(Guid? id = null) =>
    [
        (id ?? Guid.NewGuid()).ToString(),
        "checksum-abc",
        new DateTime(2026, 1, 5, 12, 0, 0),
        "ADS1 Algoritmer og datastrukturer",
        "IT-ADS1-A25",
        5,
        2,
        new DateTime(2025, 9, 1),
        new DateTime(2026, 1, 31),
        "Software Technology Engineering - Horsens",
        "Intern",
        "Nej",
        "7-trin",
        "Nej",
        "Nej",
        "Nej",
        "Nej",
        "Ja"
    ];

    public static MemoryStream Build(string[] headers, params object?[][] rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Data");

        for (var c = 0; c < headers.Length; c++)
            sheet.Cell(1, c + 1).Value = headers[c];

        for (var r = 0; r < rows.Length; r++)
            for (var c = 0; c < rows[r].Length; c++)
                sheet.Cell(r + 2, c + 1).Value = XLCellValue.FromObject(rows[r][c]);

        sheet.Range(1, 1, rows.Length + 1, headers.Length).CreateTable();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0; // rewind so the parser reads from the start
        return stream;
    }

    // Convenience: the standard headers.
    public static MemoryStream Build(params object?[][] rows) => Build(Headers, rows);

    // Returns a copy of a valid row with one column replaced (column found by header text).
    public static object?[] WithValue(object?[] row, string header, object? value)
    {
        var copy = (object?[])row.Clone();
        var index = Array.FindIndex(Headers, h => h.EndsWith(header, StringComparison.Ordinal));
        copy[index] = value;
        return copy;
    }
}
