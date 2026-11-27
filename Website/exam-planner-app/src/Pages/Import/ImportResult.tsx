import {
    COUNT_LABELS,
    MAX_ISSUES_SHOWN,
    countBySeverity,
    countKeys,
    downloadIssuesCsv,
    filterIssues,
    getImportStatus
} from "../../api/features/imports/importUtils";
import type { IssueFilter } from "../../api/features/imports/importUtils";
import type { ImportResult as ImportResultData } from "../../api/features/imports/importTypes";

interface ImportResultProps {
    result: ImportResultData;
    filter: IssueFilter;
    onFilter: (filter: IssueFilter) => void;
}

const ImportResult = ({ result, filter, onFilter }: ImportResultProps) => {
    const status = getImportStatus(result);
    const issues = filterIssues(result.issues, filter);
    const errorCount = countBySeverity(result.issues, "error");
    const warningCount = countBySeverity(result.issues, "warning");

    const filterButton = (value: IssueFilter, label: string) => (
        <button
            type="button"
            className={`via-chip ${filter === value ? "active" : ""}`}
            aria-pressed={filter === value}
            onClick={() => onFilter(value)}
        >
            {label}
        </button>
    );

    return (
        <div className="via-result">
            {/* Summary */}
            <p className={`via-status ${status.kind}`} role="status">{status.text}</p>

            <dl className="via-stats">
                <div><dt>Rows read</dt><dd>{result.totalRows}</dd></div>
                <div><dt>Imported</dt><dd>{result.importedRows}</dd></div>
                <div><dt>Skipped</dt><dd>{result.skippedRows}</dd></div>
            </dl>

            <h3 className="via-subheading">New in the database</h3>
            <dl className="via-stats">
                {countKeys.map(key => (
                    <div key={key}>
                        <dt>{COUNT_LABELS[key]}</dt>
                        <dd>{result.created[key]}</dd>
                    </div>
                ))}
            </dl>

            {/* Problems */}
            {result.issues.length > 0 && (
                <section className="via-issues">
                    <div className="via-issues-header">
                        <h3 className="via-subheading">Problems found</h3>
                        <button type="button" className="via-button-secondary via-button-small" onClick={() => downloadIssuesCsv(result)}>
                            Download as CSV
                        </button>
                    </div>

                    <div className="via-filter">
                        {filterButton("all", `All (${result.issues.length})`)}
                        {filterButton("error", `Errors (${errorCount})`)}
                        {filterButton("warning", `Warnings (${warningCount})`)}
                    </div>

                    <div className="via-table-wrap">
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
                        <p className="via-caption">
                            Showing the first {MAX_ISSUES_SHOWN} of {issues.length}. Download the CSV for the full list.
                        </p>
                    )}
                </section>
            )}
        </div>
    );
};

export default ImportResult;
