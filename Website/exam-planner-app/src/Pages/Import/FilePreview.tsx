import { useEffect, useRef, useState } from "react";
import type { MouseEvent } from "react";
import { MAX_PREVIEW_ROWS, readFilePreview } from "../../api/features/imports/filePreview";
import { formatSize } from "../../api/features/imports/importUtils";
import type { PreviewSheet } from "../../api/features/imports/importTypes";
import { FileIcon } from "./Icons.tsx";

interface FilePreviewProps {
    file: File;
    onClose: () => void;
    onImport: () => void;
}

const FilePreview = ({ file, onClose, onImport }: FilePreviewProps) => {
    const dialog = useRef<HTMLDialogElement>(null);
    const [sheets, setSheets] = useState<PreviewSheet[] | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [sheetIndex, setSheetIndex] = useState(0);
    const [showSystem, setShowSystem] = useState(false);

    useEffect(() => {
        if (dialog.current && !dialog.current.open) dialog.current.showModal();
    }, []);

    useEffect(() => {
        let cancelled = false;
        readFilePreview(file)
            .then(result => {
                if (!cancelled) setSheets(result);
            })
            .catch(() => {
                if (!cancelled) setError(`"${file.name}" could not be read as an Excel workbook.`);
            });
        return () => {
            cancelled = true;
        };
    }, [file]);

    const close = () => dialog.current?.close();

    const closeOnBackdrop = (event: MouseEvent<HTMLDialogElement>) => {
        if (event.target === dialog.current) close();
    };

    const sheet = sheets?.[sheetIndex];
    const visible = sheet
        ? sheet.columns.map((column, index) => ({ ...column, index })).filter(column => showSystem || !column.system)
        : [];
    const systemCount = sheet?.columns.filter(column => column.system).length ?? 0;

    return (
        <dialog ref={dialog} className="via-preview" aria-labelledby="via-preview-title" onClose={onClose} onClick={closeOnBackdrop}>
            <div className="via-preview-box">
                {/* Header */}
                <header className="via-preview-header">
                    <div className="via-file plain">
                        <FileIcon />
                        <div>
                            <h2 id="via-preview-title" className="via-file-name">{file.name}</h2>
                            <span className="via-file-size">{formatSize(file.size)}</span>
                        </div>
                    </div>
                    <button type="button" className="via-file-remove" aria-label="Close preview" onClick={close}>
                        ×
                    </button>
                </header>

                {/* Sheets */}
                {sheets && sheets.length > 1 && (
                    <div className="via-filter via-preview-sheets" role="tablist" aria-label="Sheets">
                        {sheets.map((item, index) => (
                            <button
                                key={item.name}
                                type="button"
                                role="tab"
                                aria-selected={index === sheetIndex}
                                className={`via-chip ${index === sheetIndex ? "active" : ""}`}
                                onClick={() => setSheetIndex(index)}
                            >
                                {item.name}
                            </button>
                        ))}
                    </div>
                )}

                {/* Table */}
                <div className="via-preview-body">
                    {!sheets && !error && <p className="via-caption">Reading the file…</p>}
                    {error && <p className="via-field-error" role="alert">{error}</p>}
                    {sheets && sheets.length === 0 && <p className="via-caption">This workbook has no visible sheets.</p>}
                    {sheet && sheet.columns.length === 0 && <p className="via-caption">Sheet "{sheet.name}" is empty.</p>}

                    {sheet && sheet.columns.length > 0 && (
                        <>
                            <div className="via-preview-info">
                                <p>
                                    Sheet "{sheet.name}": {sheet.totalRows} row{sheet.totalRows === 1 ? "" : "s"},{" "}
                                    {visible.length} column{visible.length === 1 ? "" : "s"} shown.
                                </p>
                                {systemCount > 0 && (
                                    <label className="via-check">
                                        <input type="checkbox" checked={showSystem} onChange={event => setShowSystem(event.target.checked)} />
                                        Show system columns ({systemCount})
                                    </label>
                                )}
                            </div>

                            <div className="via-table-wrap via-preview-table">
                                <table>
                                    <thead>
                                        <tr>
                                            <th>Row</th>
                                            {visible.map(column => <th key={column.index}>{column.label}</th>)}
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {sheet.rows.map(row => (
                                            <tr key={row.excelRow}>
                                                <td className="via-preview-row">{row.excelRow}</td>
                                                {visible.map(column => (
                                                    <td key={column.index} title={row.cells[column.index]}>{row.cells[column.index]}</td>
                                                ))}
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>

                            {sheet.totalRows > MAX_PREVIEW_ROWS && (
                                <p className="via-caption">Showing the first {MAX_PREVIEW_ROWS} of {sheet.totalRows} rows.</p>
                            )}
                        </>
                    )}
                </div>

                {/* Buttons */}
                <footer className="via-preview-actions">
                    <button type="button" className="via-button-secondary" onClick={close}>
                        Close
                    </button>
                    <button type="button" className="via-button" onClick={onImport} disabled={Boolean(error)}>
                        Import »
                    </button>
                </footer>
            </div>
        </dialog>
    );
};

export default FilePreview;
