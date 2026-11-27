using ExamPlannerServer.Import;

namespace ExamPlannerServer.Dto;

/// <summary>
/// Response of POST /api/v1/imports. The first six fields are the ones API_CONTRACT.md 6.1 defines.
/// The rest is the import service's own ImportResponse, passed on so the website's import dialog
/// can show the details (extra optional fields are allowed by contract rule 29.4).
/// </summary>
public record ImportSummaryDto(
    int? ImportId,
    bool Success,
    int ClassesImported,
    int StudentsImported,
    int ExamsImported,
    List<string> Warnings,
    string FileName,
    int TotalRows,
    int ImportedRows,
    int SkippedRows,
    ImportCounts Created,
    List<ImportIssue> Issues)
{
    public static ImportSummaryDto From(ImportResponse result) => new(
        ImportId: null, // the database has no import history yet
        Success: result.ImportedRows > 0,
        ClassesImported: result.Created.Classes,
        StudentsImported: result.Created.Students,
        ExamsImported: result.Created.Exams,
        Warnings: result.Issues.Select(Describe).ToList(),
        FileName: result.FileName,
        TotalRows: result.TotalRows,
        ImportedRows: result.ImportedRows,
        SkippedRows: result.SkippedRows,
        Created: result.Created,
        Issues: result.Issues);

    // "Row 12 (Kaldenavn): Kaldenavn is missing." or "File: ..." for problems with the whole file.
    private static string Describe(ImportIssue issue)
    {
        var where = issue.Row > 0 ? $"Row {issue.Row}" : "File";
        if (issue.Column is not null)
            where += $" ({issue.Column})";
        return $"{where}: {issue.Message}";
    }
}
