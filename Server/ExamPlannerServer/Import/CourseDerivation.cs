using System.Text.RegularExpressions;

namespace ExamPlannerServer.Import;

// The export has one row per offering of a course, but no separate columns for the course itself.
// These rules work out the Course values from the columns that are there. They come from reading the
// real export, and all of them live here, so a change in the rules is a change in this one file.
public static class CourseDerivation
{
    // The programme prefix: the first two letters of Kaldenavn ("IT-ADS1-A25" -> "IT").
    // Null when Kaldenavn does not start with two letters.
    public static string? Prefix(string nickname)
    {
        var text = nickname.Trim();
        return text.Length >= 2 && char.IsLetter(text[0]) && char.IsLetter(text[1])
            ? text[..2].ToUpperInvariant()
            : null;
    }

    // The course code: the first word of Navn, cut at a hyphen.
    // "ADS1 Algoritmer og datastrukturer" -> "ADS1", "BET1-Sommer - Grundlæggende ..." -> "BET1".
    // Navn is used (not Kaldenavn) because Kaldenavn sometimes has extras, like "IT-PRO2,1.0-S25".
    public static string? CourseCode(string name)
    {
        var code = FirstWord(name).Split('-')[0].Trim();
        return code.Length > 0 ? code : null;
    }

    // Navn without its first word, because the code has its own column.
    // "ADS1 Algoritmer og datastrukturer" -> "Algoritmer og datastrukturer".
    // If nothing is left, the full Navn is kept.
    public static string CourseName(string name)
    {
        var text = name.Trim();
        var rest = text[FirstWord(text).Length..].TrimStart(' ', '\t', '-', '–', '—');
        return rest.Length > 0 ? rest : text;
    }

    // The campus, taken from the end of Aktivitetsudbud:
    // "Software Technology Engineering - VIA University College, Campus Horsens (online) " -> "Horsens".
    // Empty when the text has no ", Campus ..." at its end. The comma and the capital C are required on purpose:
    // a loose match could pick up a stray word and save a wrong campus without anyone noticing,
    // while an empty campus is reported as a warning.
    public static string Campus(string activityOffering)
    {
        var match = CampusPattern.Match(activityOffering);
        return match.Success ? match.Groups[1].Value.Trim() : "";
    }

    // Two rows belong to the same course when programme and code are the same.
    public static string CourseKey(string prefix, string courseCode) =>
        prefix.ToUpperInvariant() + "|" + courseCode.ToUpperInvariant();

    private static readonly Regex CampusPattern =
        new(@",\s*Campus\s+(.+?)\s*(\([^)]*\))?\s*$", RegexOptions.Compiled);

    private static string FirstWord(string text)
    {
        var trimmed = text.Trim();
        var end = trimmed.IndexOfAny([' ', '\t']);
        return end < 0 ? trimmed : trimmed[..end];
    }
}
