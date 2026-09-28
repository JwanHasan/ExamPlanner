export type IssueSeverity = "error" | "warning";

export interface ImportIssue {
    severity: IssueSeverity;
    sheet: string;
    row: number;
    column?: string;
    message: string;
}

export interface ImportCounts {
    courses: number;
    classes: number;
    students: number;
    enrollments: number;
    lecturers: number;
    exams: number;
}

export interface ImportResult {
    fileName: string;
    totalRows: number;
    importedRows: number;
    skippedRows: number;
    created: ImportCounts;
    issues: ImportIssue[];
}