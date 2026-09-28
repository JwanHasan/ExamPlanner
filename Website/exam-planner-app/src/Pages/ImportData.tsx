import {
    useState
} from "react";

import PageShell
    from "../components/PageShell";

import "./Themes/Plan.css";

const ImportData = () => {

    const [
        fileName,
        setFileName
    ] =
        useState<string | null>(
            null
        );

    const [
        message,
        setMessage
    ] =
        useState<string | null>(
            null
        );

    return (
        <PageShell
            title="Import data"
            description={
                "Select the Excel export used to prepare the exam plan."
            }
        >

            <div
                className={
                    "two-column-layout"
                }
            >

                <section
                    className="panel-card"
                >

                    <h2>
                        Select source file
                    </h2>

                    <label
                        className={
                            "upload-box"
                        }
                    >

                        <strong>
                            {
                                fileName ??
                                "Choose an Excel file"
                            }
                        </strong>

                        <span>
                            .xlsx files
                            are expected
                        </span>

                        <input
                            type="file"
                            accept=".xlsx"
                            onChange={
                                event => {

                                    setFileName(
                                        event
                                            .target
                                            .files?.[0]
                                            ?.name ??
                                        null
                                    );

                                    setMessage(
                                        null
                                    );
                                }
                            }
                        />

                    </label>

                    <button
                        className={
                            "primary-button"
                        }
                        type="button"
                        disabled={
                            !fileName
                        }
                        onClick={
                            () =>
                                setMessage(
                                    "File selected. " +
                                    "Actual upload/validation " +
                                    "will be connected when " +
                                    "the import endpoint is implemented."
                                )
                        }
                    >
                        Validate and import
                    </button>

                    {message && (
                        <p
                            className={
                                "success-message"
                            }
                            role="status"
                        >
                            {message}
                        </p>
                    )}

                </section>

                <aside
                    className="panel-card"
                >

                    <h2>
                        Expected process
                    </h2>

                    <ol
                        className={
                            "steps-list"
                        }
                    >

                        <li>
                            Select the latest
                            Excel export.
                        </li>

                        <li>
                            Validate mandatory
                            columns and values.
                        </li>

                        <li>
                            Show warnings or
                            rejected rows.
                        </li>

                        <li>
                            Import valid data
                            into the planning
                            database.
                        </li>

                    </ol>

                </aside>

            </div>

        </PageShell>
    );
};

export default ImportData;