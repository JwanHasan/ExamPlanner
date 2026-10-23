import { useRef, useState } from "react";
import type { ChangeEvent, DragEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../api/features/auth/AuthContext.tsx";
import { useImport } from "../../api/features/imports/useImport";
import { ALLOWED_EXTENSIONS, checkFile, formatSize, progressLabel } from "../../api/features/imports/importUtils";
import type { IssueFilter } from "../../api/features/imports/importUtils";
import { FileIcon } from "./Icons.tsx";
import StepIndicator from "./StepIndicator.tsx";
import ImportResult from "./ImportResult.tsx";
import viaLogo from "../Assets/via-logo-small.png";
import "../Themes/ImportPage.css";

const STEPS = ["Choose file", "Import", "Result"];

const INTRO: Record<number, { title: string; text: string }> = {
    1: { title: "Upload", text: "Select the Excel export you want to import into Exam Plan." },
    2: { title: "Import", text: "The file is being uploaded and checked. Large files can take a moment." },
    3: { title: "Result", text: "This is what the import found. Rows with errors were skipped; the other rows were saved." }
};

const ImportPage = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const importer = useImport();
    const fileInput = useRef<HTMLInputElement>(null);

    const [file, setFile] = useState<File | null>(null);
    const [fileError, setFileError] = useState<string | null>(null);
    const [dragging, setDragging] = useState(false);
    const [filter, setFilter] = useState<IssueFilter>("all");

    const { busy, progress, fileName, result, error } = importer.state;
    const step = busy ? 2 : result || error ? 3 : 1;
    const intro = step === 3 && !result
        ? { title: "Result", text: "The file could not be imported." }
        : INTRO[step];

    const pickFile = (chosen: File | undefined) => {
        if (!chosen) return;
        const problem = checkFile(chosen);
        setFileError(problem);
        setFile(problem ? null : chosen);
    };

    const handleFileInput = (event: ChangeEvent<HTMLInputElement>) => {
        pickFile(event.target.files?.[0]);
        event.target.value = "";
    };

    const handleDragOver = (event: DragEvent<HTMLDivElement>) => {
        event.preventDefault();
        setDragging(true);
    };

    const handleDrop = (event: DragEvent<HTMLDivElement>) => {
        event.preventDefault();
        setDragging(false);
        pickFile(event.dataTransfer.files[0]);
    };

    const startImport = () => {
        if (!file) return;
        setFilter("all");
        void importer.start(file);
    };

    const importAnother = () => {
        importer.close();
        setFile(null);
        setFileError(null);
    };

    return (
        <div className="via-page">
            {/* Top bar */}
            <header className="via-topbar">
                <Link className="via-brand" to="/Main">
                    <img src={viaLogo} alt="VIA University College" />
                    <span>Exam Plan</span>
                </Link>
                {user && (
                    <span className="via-user" title={user.email}>
                        {user.name || user.email}
                    </span>
                )}
            </header>

            {/* Title bar */}
            <div className="via-titlebar">
                <h1>Import data</h1>
                <StepIndicator steps={STEPS} current={step} />
            </div>

            <main className="via-content">
                <div className="via-columns">
                    {/* Intro */}
                    <section className="via-intro">
                        <h2>{intro.title}</h2>
                        <p>{intro.text}</p>
                    </section>

                    <section className="via-work">
                        {/* Step 1: choose file */}
                        {step === 1 && (
                            <>
                                <div
                                    className={`via-dropzone ${dragging ? "dragging" : ""}`}
                                    onDragOver={handleDragOver}
                                    onDragLeave={() => setDragging(false)}
                                    onDrop={handleDrop}
                                >
                                    {file ? (
                                        <div className="via-file">
                                            <FileIcon />
                                            <div>
                                                <span className="via-file-name">{file.name}</span>
                                                <span className="via-file-size">{formatSize(file.size)}</span>
                                            </div>
                                            <button
                                                type="button"
                                                className="via-file-remove"
                                                aria-label={`Remove ${file.name}`}
                                                onClick={() => setFile(null)}
                                            >
                                                ×
                                            </button>
                                        </div>
                                    ) : (
                                        <>
                                            <FileIcon className="via-dropzone-icon" />
                                            <p className="via-dropzone-text">Drag files here</p>
                                        </>
                                    )}

                                    <button type="button" className="via-button" onClick={() => fileInput.current?.click()}>
                                        {file ? "Choose another file" : "Upload from computer"}
                                    </button>

                                    <input
                                        ref={fileInput}
                                        type="file"
                                        accept={ALLOWED_EXTENSIONS.join(",")}
                                        onChange={handleFileInput}
                                        hidden
                                    />
                                </div>

                                {fileError && <p className="via-field-error" role="alert">{fileError}</p>}

                                <p className="via-caption">
                                    The following file types are allowed: Microsoft Excel xlsx, xlsm. Maximum size 25 MB.
                                </p>
                            </>
                        )}

                        {/* Step 2: upload */}
                        {step === 2 && (
                            <div className="via-progress-card">
                                <div className="via-file plain">
                                    <FileIcon />
                                    <div>
                                        <span className="via-file-name">{fileName}</span>
                                        {file && <span className="via-file-size">{formatSize(file.size)}</span>}
                                    </div>
                                </div>
                                <div className="via-progress" role="progressbar" aria-valuemin={0} aria-valuemax={100} aria-valuenow={progress}>
                                    <div className="via-progress-bar" style={{ width: `${progress}%` }} />
                                </div>
                                <p className="via-caption">{progressLabel(progress)}</p>
                            </div>
                        )}

                        {/* Step 3: result */}
                        {step === 3 && error && <p className="via-status failed" role="alert">{error}</p>}
                        {step === 3 && result && <ImportResult result={result} filter={filter} onFilter={setFilter} />}
                    </section>
                </div>

                {/* Buttons */}
                <footer className="via-actions">
                    {step === 3 ? (
                        <>
                            <button type="button" className="via-button-secondary" onClick={importAnother}>
                                « Import another file
                            </button>
                            <button type="button" className="via-button via-button-large" onClick={() => navigate("/Main")}>
                                Back to main page »
                            </button>
                        </>
                    ) : (
                        <>
                            <button type="button" className="via-button-secondary" onClick={() => navigate("/Main")} disabled={busy}>
                                « Main page
                            </button>
                            <button type="button" className="via-button via-button-large" onClick={startImport} disabled={!file || busy}>
                                {busy ? "Importing" : "Import »"}
                            </button>
                        </>
                    )}
                </footer>
            </main>
        </div>
    );
};

export default ImportPage;
