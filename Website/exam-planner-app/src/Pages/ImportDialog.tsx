import { useEffect, useState } from "react";
import type { ImportState } from "../api/features/imports/useImport";
import { useCloseOnEscape } from "../api/features/imports/useCloseOnEscape";
import {
    COUNT_LABELS,
    MAX_ISSUES_SHOWN,
    countBySeverity,
    countKeys,
    downloadIssuesCsv,
    filterIssues,
    getImportStatus,
    progressLabel
} from "../api/features/imports/importUtils";
import type { IssueFilter } from "../api/features/imports/importUtils";
import "./Themes/ImportDialog.css";

interface ImportDialogProps {
    state: ImportState;
    onClose: () => void;
}

const ImportDialog = ({ state, onClose }: ImportDialogProps) => {
    const { open, busy, progress, fileName, result, error } = state;
    const [filter, setFilter] = useState<IssueFilter>("all");

    useCloseOnEscape(open && !busy, onClose);
    useEffect(() => setFilter("all"), [result]);

    if (!open) return null;

    const status = result && getImportStatus(result);
    const issues = result ? filterIssues(result.issues, filter) : [];
    const errorCount = result ? countBySeverity(result.issues, "error") : 0;
    const warningCount = result ? countBySeverity(result.issues, "warning") : 0;

    const filterButton = (value: IssueFilter, label: string) => (
        <button
            type="button"
            className={`import-small-button ${filter === value ? "active" : ""}`}
            onClick={() => setFilter(value)}
        >
            {label}
        </button>
    );

    return (
        <div className="import-overlay" onClick={() => !busy && onClose()}>
            <div
                className="import-dialog"
                role="dialog"
                aria-modal="true"
                aria-labelledby="import-dialog-title"
                onClick={e => e.stopPropagation()}
            >
                <div className="import-dialog-header">
                    <h2 id="import-dialog-title">Import: {fileName}</h2>
                    <button type="button" className="import-dialog-close" onClick={onClose} disabled={busy} aria-label="Close">
                        ×
                    </button>
                </div>

                {busy && (
                    <div className="import-progress">
                        <div className="import-progress-bar" style={{ width: `${progress}%` }} />
                        <span>{progressLabel(progress)}</span>
                    </div>
                )}

                {error && <p className="import-status failed" role="alert">{error}</p>}

                {result && status && (
                    <>
                        <p className={`import-status ${status.kind}`} role="status">{status.text}</p>

                        <dl className="import-grid">
                            <div><dt>Rows read</dt><dd>{result.totalRows}</dd></div>
                            <div><dt>Imported</dt><dd>{result.importedRows}</dd></div>
                            <div><dt>Skipped</dt><dd>{result.skippedRows}</dd></div>
                        </dl>

                        <dl className="import-grid">
                            {countKeys.map(key => (
                                <div key={key}>
                                    <dt>{COUNT_LABELS[key]}</dt>
                                    <dd>{result.created[key]}</dd>
                                </div>
                            ))}
                        </dl>

                        {result.issues.length > 0 && (
                            <section className="import-issues">
                                <div className="import-issues-header">
                                    <h3>Problems found</h3>
                                    <button type="button" className="import-small-button" onClick={() => downloadIssuesCsv(result)}>
                                        Download as CSV
                                    </button>
                                </div>

                                <div className="import-filter">
                                    {filterButton("all", `All (${result.issues.length})`)}
                                    {filterButton("error", `Errors (${errorCount})`)}
                                    {filterButton("warning", `Warnings (${warningCount})`)}
                                </div>

                                <div className="import-table-wrap">
                                    <table>
                                        <thead>
                                        <tr>
                                            <th>Type</th>
                                            <th>Sheet</th>
                                            <th>Row</th>
                                            <th>Column</th>
                                            <th>Problem</th>
                                        </tr>
                                        </thead>
                                        <tbody>
                                        {issues.slice(0, MAX_ISSUES_SHOWN).map((issue, index) => (
                                            <tr key={index} className={issue.severity}>
                                                <td>{issue.severity === "error" ? "Error" : "Warning"}</td>
                                                <td>{issue.sheet}</td>
                                                <td>{issue.row}</td>
                                                <td>{issue.column ?? ""}</td>
                                                <td>{issue.message}</td>
                                            </tr>
                                        ))}
                                        </tbody>
                                    </table>
                                </div>

                                {issues.length > MAX_ISSUES_SHOWN && (
                                    <p className="import-more">
                                        Showing the first {MAX_ISSUES_SHOWN} of {issues.length}. Download the CSV for the full list.
                                    </p>
                                )}
                            </section>
                        )}
                    </>
                )}
            </div>
        </div>
    );
};

export default ImportDialog;