using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ExamPlannerServer.Import;

// Reads the Opslagsark: the sheet with one row per student per planning element.
// Like the planning-element parser it finds its table and columns by name, never by position.
public class StudentRegistrationParser : IStudentRegistrationParser
{
    private const string ColRegistrationId = "Gennemførelsesuddannelseselement";
    // Public: the importer recognises an Opslagsark by this column.
    public const string ColStudent = "Anonymt Student-ID";
    private const string ColElementNickname = "Kaldenavn (Planlægningsuddannelseselement)";
    private const string ColElementName = "Planlægningsuddannelseselement";
    private const string ColHold = "Hold";
    private const string ColAttempts = "Antal forsøg";

    // Public so the importer can tell which kind of file it was given (this one has the student column).
    public static readonly IReadOnlyList<string> RequiredColumns =
    [
        ColRegistrationId, ColStudent, ColElementNickname, ColElementName, ColHold, ColAttempts
    ];

    public StudentRegistrationParseResult Parse(Stream fileStream)
    {
        var result = new StudentRegistrationParseResult();

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

    private static void ReadRows(IXLTable table, Dictionary<string, int> columns, StudentRegistrationParseResult result)
    {
        var seenIds = new HashSet<Guid>();
        var seenPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in table.DataRange.Rows())
        {
            if (row.IsEmpty())
                continue; // a blank row inside the table is not an error

            var reader = new RowReader(row, columns);

            var id = reader.Guid(ColRegistrationId);
            var studentCode = reader.Text(ColStudent);
            var nickname = reader.Text(ColElementNickname);
            var parsed = new ParsedStudentRegistrationRow
            {
                SourceRowNumber = row.RowNumber(),
                RegistrationId = id,
                StudentCode = studentCode,
                PlanningElementNickname = nickname,
                PlanningElementName = reader.Text(ColElementName),
                Hold = reader.Text(ColHold),
                AttemptNo = reader.Int(ColAttempts)
            };

            // The student's number is the digits at the end of the id ("STU-0001" -> 1).
            if (studentCode.Length > 0)
            {
                if (TryStudentNumber(studentCode, out var number))
                    parsed.StudentNumber = number;
                else
                    reader.Problems.Add($"{ColStudent}: '{studentCode}' has no number at the end");
            }

            // Checks that need several values at once only make sense if they were readable.
            if (reader.Problems.Count == 0)
            {
                if (!seenIds.Add(id))
                    reader.Problems.Add($"{ColRegistrationId} {id} appears more than once in the file");

                // A student is registered on a planning element once. Two rows would clash when saved.
                if (!seenPairs.Add(studentCode + "|" + nickname))
                    reader.Problems.Add($"{studentCode} is registered on '{nickname}' more than once in the file");
            }

            if (reader.Problems.Count > 0)
                result.Issues.Add(new ParseIssue(parsed.SourceRowNumber, string.Join("; ", reader.Problems)));
            else
                result.Rows.Add(parsed);
        }
    }

    private static bool TryStudentNumber(string studentCode, out int number)
    {
        var match = TrailingNumber.Match(studentCode);
        number = 0;
        return match.Success && int.TryParse(match.Value, out number);
    }

    private static readonly Regex TrailingNumber = new(@"\d+$", RegexOptions.Compiled);
}
