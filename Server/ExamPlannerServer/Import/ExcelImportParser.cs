using ClosedXML.Excel;

namespace ExamPlannerServer.Import;

public class ExcelImportParser : IExcelImportParser
{
    // Prefix the Dynamics export puts in front of its three system columns.
    private const string SystemColumnPrefix = "(Skal ikke ændres)";

    // Every column the import needs, written the way the headers look AFTER Normalize()
    // (no "(Skal ikke ændres)" prefix). Columns are found by these names, never by position,
    // so a re-export with reordered or extra columns still works.
    private const string ColId = "Planlægningsuddannelseselement";
    private const string ColChecksum = "Kontrolsum for række";
    private const string ColModified = "Ændret";
    private const string ColName = "Navn";
    private const string ColNickname = "Kaldenavn";
    private const string ColEcts = "ECTS";
    private const string ColExecutionCount = "Antal gennemførelsesuddannelseselementer";
    private const string ColStartDate = "Startdato";
    private const string ColEndDate = "Slutdato";
    private const string ColActivityOffering = "Aktivitetsudbud";
    private const string ColAssessmentType = "Bedømmelsestype";
    private const string ColCompletion = "Gennemførelse";
    private const string ColGradingScale = "Karakterskala";
    private const string ColCombinedWrittenOral = "Kombineret skriftlig og mundtlig";
    private const string ColOral = "Mundtlig";
    private const string ColPractical = "Praktisk prøve";
    private const string ColProject = "Projekt";
    private const string ColWritten = "Skriftlig";

    private static readonly string[] RequiredColumns =
    [
        ColId, ColChecksum, ColModified, ColName, ColNickname, ColEcts, ColExecutionCount,
        ColStartDate, ColEndDate, ColActivityOffering, ColAssessmentType, ColCompletion,
        ColGradingScale, ColCombinedWrittenOral, ColOral, ColPractical, ColProject, ColWritten
    ];

    public ParseResult Parse(Stream fileStream)
    {
        var result = new ParseResult();

        // Opening can fail for many reasons (not an xlsx, corrupt, password protected) and
        // ClosedXML throws different exception types for them. For a user-facing import we
        // turn any of them into one clear error instead of crashing the request.
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(fileStream);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"The file could not be read as an Excel (.xlsx) workbook: {ex.Message}");
            return result;
        }

        using (workbook)
        {
            var found = FindImportTable(workbook, result);
            if (found is null)
                return result; // the reason is in result.Errors

            ReadRows(found.Value.Table, found.Value.Columns, result);
        }

        return result;
    }

    // Looks through every table on every sheet (sheet names and positions are not trusted)
    // and returns the first one that has all required columns.
    private static (IXLTable Table, Dictionary<string, int> Columns)? FindImportTable(
        XLWorkbook workbook, ParseResult result)
    {
        List<string>? fewestMissing = null;

        foreach (var table in workbook.Worksheets.SelectMany(ws => ws.Tables))
        {
            var columns = MapColumns(table);
            var missing = RequiredColumns.Where(c => !columns.ContainsKey(c)).ToList();

            if (missing.Count == 0)
                return (table, columns);

            // Remember the closest candidate so the error message is useful.
            if (fewestMissing is null || missing.Count < fewestMissing.Count)
                fewestMissing = missing;
        }

        result.Errors.Add(fewestMissing is null
            ? "No table was found in the workbook."
            : $"Required column(s) missing: {string.Join(", ", fewestMissing)}");
        return null;
    }

    // header name -> column number inside the table (1-based, which is what row.Cell(n) expects)
    private static Dictionary<string, int> MapColumns(IXLTable table)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headers = table.HeadersRow().Cells().ToList();

        for (var i = 0; i < headers.Count; i++)
            map.TryAdd(Normalize(headers[i].GetString()), i + 1);

        return map;
    }

    // Trims, and removes the "(Skal ikke ændres)" prefix from the three system columns.
    private static string Normalize(string header)
    {
        var text = header.Trim();
        if (text.StartsWith(SystemColumnPrefix, StringComparison.OrdinalIgnoreCase))
            text = text[SystemColumnPrefix.Length..].Trim();
        return text;
    }

    private static void ReadRows(IXLTable table, Dictionary<string, int> columns, ParseResult result)
    {
        var seenIds = new HashSet<Guid>();

        foreach (var row in table.DataRange.Rows())
        {
            if (row.IsEmpty())
                continue; // a blank row inside the table is not an error

            var reader = new RowReader(row, columns);

            // Each read adds to reader.Problems when the cell is missing or has the wrong type,
            // so we collect ALL problems of a row, not just the first.
            var id = reader.Guid(ColId);
            var parsed = new ParsedPlanningEducationElementRow
            {
                SourceRowNumber = row.RowNumber(),
                PlanningElementId = id,
                SourceChecksum = reader.Text(ColChecksum),
                SourceModifiedAt = reader.Date(ColModified),
                Name = reader.Text(ColName),
                Nickname = reader.Text(ColNickname),
                Ects = reader.Decimal(ColEcts),
                ExecutionElementCount = reader.Int(ColExecutionCount),
                StartDate = reader.Date(ColStartDate),
                EndDate = reader.Date(ColEndDate),
                ActivityOffering = reader.Text(ColActivityOffering),
                AssessmentType = reader.Text(ColAssessmentType),
                CompletionAssessment = reader.Bool(ColCompletion),
                GradingScale = reader.Text(ColGradingScale),
                CombinedWrittenOral = reader.Bool(ColCombinedWrittenOral),
                Oral = reader.Bool(ColOral),
                PracticalExam = reader.Bool(ColPractical),
                Project = reader.Bool(ColProject),
                Written = reader.Bool(ColWritten)
            };

            // Checks that need several values at once only make sense if they were readable.
            if (reader.Problems.Count == 0)
            {
                if (parsed.EndDate < parsed.StartDate)
                    reader.Problems.Add($"{ColEndDate} is before {ColStartDate}");

                // The id is the key for updating existing data later, so it must be unique in the file.
                if (!seenIds.Add(id))
                    reader.Problems.Add($"{ColId} {id} appears more than once in the file");
            }

            if (reader.Problems.Count > 0)
                result.Errors.Add($"Row {parsed.SourceRowNumber}: {string.Join("; ", reader.Problems)}");
            else
                result.Rows.Add(parsed);
        }
    }

    // Reads typed values out of one row's cells and records a problem instead of throwing
    // when a value is missing or of the wrong type.
    private sealed class RowReader(IXLRangeRow row, Dictionary<string, int> columns)
    {
        public List<string> Problems { get; } = new();

        private IXLCell Cell(string column) => row.Cell(columns[column]);

        public string Text(string column)
        {
            var text = Cell(column).GetString().Trim();
            if (text.Length == 0)
                Problems.Add($"{column}: value is missing");
            return text;
        }

        public Guid Guid(string column)
        {
            var text = Cell(column).GetString().Trim();
            if (System.Guid.TryParse(text, out var value))
                return value;
            Problems.Add($"{column}: '{text}' is not a valid id");
            return default;
        }

        // Only accepts cells Excel itself holds as dates. The display text is never parsed,
        // because its format depends on the machine's language settings.
        public DateTime Date(string column)
        {
            var cell = Cell(column);
            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime();
            Problems.Add($"{column}: '{cell.GetString()}' is not a date");
            return default;
        }

        public decimal Decimal(string column)
        {
            var cell = Cell(column);
            if (cell.DataType == XLDataType.Number)
                return (decimal)cell.GetDouble();
            Problems.Add($"{column}: '{cell.GetString()}' is not a number");
            return default;
        }

        public int Int(string column)
        {
            var cell = Cell(column);
            if (cell.DataType == XLDataType.Number)
            {
                var number = cell.GetDouble();
                if (number >= 0 && number % 1 == 0)
                    return (int)number;
            }
            Problems.Add($"{column}: '{cell.GetString()}' is not a whole number");
            return default;
        }

        // "Ja" / "Nej" (any capitalisation, surrounding spaces ignored); anything else is a problem.
        public bool Bool(string column)
        {
            var text = Cell(column).GetString().Trim();
            if (text.Equals("Ja", StringComparison.OrdinalIgnoreCase))
                return true;
            if (text.Equals("Nej", StringComparison.OrdinalIgnoreCase))
                return false;
            Problems.Add($"{column}: '{text}' is not Ja or Nej");
            return default;
        }
    }
}
