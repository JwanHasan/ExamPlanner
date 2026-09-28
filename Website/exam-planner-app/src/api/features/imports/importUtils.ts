import type { ImportCounts, ImportIssue, ImportResult, IssueSeverity } from "./importTypes";

export type IssueFilter = "all" | IssueSeverity;

export type ImportStatusKind = "success" | "partial" | "failed";

export interface ImportStatus {
    kind: ImportStatusKind;
    text: string;
}


export const MAX_ISSUES_SHOWN = 200;

export const COUNT_LABELS: Record<keyof ImportCounts, string> = {
    courses: "Courses",
    classes: "Classes",
    students: "Students",
    enrollments: "Enrollments",
    lecturers: "Lecturers",
    exams: "Exams"
};

export const countKeys = Object.keys(COUNT_LABELS) as (keyof ImportCounts)[];

export const countBySeverity = (issues: ImportIssue[], severity: IssueSeverity): number =>
    issues.filter(i => i.severity === severity).length;

export const filterIssues = (issues: ImportIssue[], filter: IssueFilter): ImportIssue[] =>
    filter === "all" ? issues : issues.filter(i => i.severity === filter);

/** One-line verdict shown at the top of the result. */
export const getImportStatus = (result: ImportResult): ImportStatus => {
    const errors = countBySeverity(result.issues, "error");

    if (result.importedRows === 0) {
        return { kind: "failed", text: "Nothing was imported." };
    }
    if (errors > 0) {
        return {
            kind: "partial",
            text: `Imported with problems: ${errors} row${errors === 1 ? "" : "s"} skipped.`
        };
    }
    return { kind: "success", text: "Import completed without errors." };
};

export const progressLabel = (percent: number): string =>
    percent < 100 ? `Uploading ${percent}%` : "Processing on the server";

/** Saves the issues as a CSV the administrators can open in Excel. */
export const downloadIssuesCsv = (result: ImportResult): void => {
    const escape = (value: string | number | undefined) =>
        `"${String(value ?? "").replace(/"/g, '""')}"`;

    const lines = [
        ["Severity", "Sheet", "Row", "Column", "Message"].join(";"),
        ...result.issues.map(i =>
            [i.severity, i.sheet, i.row, i.column, i.message].map(escape).join(";")
        )
    ];

    // The BOM makes Excel read Danish letters (æ, ø, å) correctly.
    const blob = new Blob(["\uFEFF" + lines.join("\n")], { type: "text/csv;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = result.fileName.replace(/\.xlsx?$/i, "") + "-import-issues.csv";
    link.click();
    URL.revokeObjectURL(url);
};