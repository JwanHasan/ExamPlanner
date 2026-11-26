namespace TesProject;

// Builds small Opslagsark-shaped workbooks in memory (see TestWorkbook for the planning-element export).
// The headers are the real ones, including Dynamics' habit of repeating the related table's name in brackets.
internal static class StudentWorkbook
{
    public const string RegistrationId = "Gennemførelsesuddannelseselement";
    public const string Student = "Anonymt Student-ID";
    public const string Nickname = "Kaldenavn (Planlægningsuddannelseselement)";
    public const string Hold = "Hold";
    public const string Attempts = "Antal forsøg";

    public static readonly string[] Headers =
    [
        "(Skal ikke ændres) Gennemførelsesuddannelseselement",
        "(Skal ikke ændres) Kontrolsum for række",
        "(Skal ikke ændres) Ændret",
        "Navn",
        "Semester",
        "Kaldenavn (Planlægningsuddannelseselement) (Planlægningsuddannelseselement)",
        "Planlægningsuddannelseselement",
        "Aktivitetsudbud (Planlægningsuddannelseselement) (Planlægningsuddannelseselement)",
        "Antal forsøg",
        "Hold",
        "Anonymt Student-ID",
        "Præfix"
    ];

    private static int _counter;

    // One valid row, values in the same order as Headers.
    public static object?[] ValidRow(
        Guid? id = null, string student = "STU-0001", string nickname = "KA61-BDL-S26",
        string hold = "KA61-BDL-F26", int attempts = 0) =>
    [
        (id ?? Guid.NewGuid()).ToString(),
        "checksum-abc",
        new DateTime(2026, 2, 11, 12, 24, 0),
        "BDLV6 Bæredygtig ledelse - STU-0001 - Procesøkonomi og værdikædeledelse",
        6,
        nickname,
        "BDLV6 Bæredygtig ledelse",
        "Procesøkonomi og værdikædeledelse - VIA University College, Campus Aarhus C",
        attempts,
        hold,
        student,
        "KA"
    ];

    // A row with its own student and its own planning element, so it never clashes with another row.
    public static object?[] UniqueRow(Guid? id = null, int attempts = 0)
    {
        var n = Interlocked.Increment(ref _counter);
        return ValidRow(id, student: $"STU-{n:0000}", nickname: $"KA61-U{n}-S26", hold: $"KA61-U{n}-F26", attempts: attempts);
    }

    // Returns a copy of a row with one column replaced (column found by header text).
    public static object?[] WithValue(object?[] row, string header, object? value)
    {
        var copy = (object?[])row.Clone();
        var index = Array.FindIndex(Headers, h => h.StartsWith(header, StringComparison.Ordinal) || h.EndsWith(header, StringComparison.Ordinal));
        copy[index] = value;
        return copy;
    }

    public static MemoryStream Build(params object?[][] rows) => TestWorkbook.Build(Headers, rows);
}

// The real Opslagsark (anonymised). Like the real planning-element export it is not in the repository:
// tests that need it skip themselves when it is missing. Set the path to where your copy is.
public static class OpslagsarkFile
{
    public const string Path = @"C:\Users\lsuga\Downloads\Opslagsark til planlægning - ANONYMISERET.xlsx";
}

public sealed class RealOpslagsarkFactAttribute : FactAttribute
{
    public RealOpslagsarkFactAttribute()
    {
        if (!File.Exists(OpslagsarkFile.Path))
            Skip = $"Real Opslagsark not found at {OpslagsarkFile.Path}";
    }
}
