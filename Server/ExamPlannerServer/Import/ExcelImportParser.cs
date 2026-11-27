using ClosedXML.Excel;

namespace ExamPlannerServer.Import;

public class ExcelImportParser : IExcelImportParser
{
    // Every column the import needs, written the way the headers look AFTER normalising
    // (no "(Skal ikke ændres)" prefix). Columns are found by these names, not by position,
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
            result.Issues.Add(new ParseIssue(0, $"The file could not be read as an Excel (.xlsx) workbook: {ex.Message}"));
            return result;
        }

        using (workbook)
        {
            if (!ExcelTable.TryFind(workbook, RequiredColumns, out var found, out var error))
            {
                result.Issues.Add(new ParseIssue(0, error!));
                return result;
            }

            result.SheetName = found!.Table.Worksheet.Name;
            ReadRows(found.Table, found.Columns, result);
        }

        return result;
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
                result.Issues.Add(new ParseIssue(parsed.SourceRowNumber, string.Join("; ", reader.Problems)));
            else
                result.Rows.Add(parsed);
        }
    }
}
