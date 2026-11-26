using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ExamPlannerServer.Import;

// What the Excel parsers have in common: finding the data table of a workbook by its column names.
internal static class ExcelTable
{
    // Prefix the Dynamics export puts in front of its system columns.
    private const string SystemColumnPrefix = "(Skal ikke ændres)";

    public sealed record Found(IXLTable Table, Dictionary<string, int> Columns);

    // Looks through every table on every sheet (sheet names and positions are not trusted)
    // and returns the first one that has all required columns. If there is none, `error` says why.
    public static bool TryFind(
        XLWorkbook workbook, IReadOnlyCollection<string> requiredColumns, out Found? found, out string? error)
    {
        List<string>? fewestMissing = null;

        foreach (var table in workbook.Worksheets.SelectMany(ws => ws.Tables))
        {
            var columns = MapColumns(table);
            var missing = requiredColumns.Where(c => !columns.ContainsKey(c)).ToList();

            if (missing.Count == 0)
            {
                found = new Found(table, columns);
                error = null;
                return true;
            }

            // Remember the closest candidate so the error message is useful.
            if (fewestMissing is null || missing.Count < fewestMissing.Count)
                fewestMissing = missing;
        }

        found = null;
        error = fewestMissing is null
            ? "No table was found in the workbook."
            : $"Required column(s) missing: {string.Join(", ", fewestMissing)}";
        return false;
    }

    // True when some table in the workbook has this column. Used to tell which kind of file was uploaded.
    public static bool HasColumn(XLWorkbook workbook, string column) =>
        workbook.Worksheets.SelectMany(ws => ws.Tables).Any(table => MapColumns(table).ContainsKey(column));

    // header name -> column number inside the table (1-based, which is what row.Cell(n) expects)
    private static Dictionary<string, int> MapColumns(IXLTable table)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headers = table.HeadersRow().Cells().ToList();

        for (var i = 0; i < headers.Count; i++)
            map.TryAdd(Normalize(headers[i].GetString()), i + 1);

        return map;
    }

    // Trims, removes the "(Skal ikke ændres)" prefix from the system columns, and collapses a repeated
    // bracket: Dynamics writes "Kaldenavn (Planlægningsuddannelseselement) (Planlægningsuddannelseselement)"
    // for a column that comes from a related record, and we call it "Kaldenavn (Planlægningsuddannelseselement)".
    private static string Normalize(string header)
    {
        var text = header.Trim();
        if (text.StartsWith(SystemColumnPrefix, StringComparison.OrdinalIgnoreCase))
            text = text[SystemColumnPrefix.Length..].Trim();
        return RepeatedBracket.Replace(text, "$1");
    }

    private static readonly Regex RepeatedBracket = new(@"(\([^)]+\))(?:\s*\1)+", RegexOptions.Compiled);
}
