using ClosedXML.Excel;

namespace ExamPlannerServer.Import;

// Reads typed values out of one row's cells and records a problem instead of throwing
// when a value is missing or of the wrong type.
internal sealed class RowReader(IXLRangeRow row, Dictionary<string, int> columns)
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
