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

// ---------- File preview ----------

export interface PreviewColumn {
    label: string;
    system: boolean;
}

export interface PreviewRow {
    excelRow: number;
    cells: string[];
}

export interface PreviewSheet {
    name: string;
    columns: PreviewColumn[];
    rows: PreviewRow[];
    totalRows: number;
}
