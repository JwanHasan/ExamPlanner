import {
    useMemo,
    useState
} from "react";

import PageShell
    from "../components/PageShell";

import {
    examSessions
} from "../data/mockData";

import type {
    ExamSessionRow
} from "../data/mockData";

import "./Themes/Plan.css";

const EditPlan = () => {

    const [
        rows,
        setRows
    ] =
        useState<ExamSessionRow[]>(
            examSessions
        );

    const [
        selectedId,
        setSelectedId
    ] =
        useState<number | null>(
            null
        );

    const [
        newDate,
        setNewDate
    ] =
        useState("");

    const [
        newRoom,
        setNewRoom
    ] =
        useState("");

    const [
        notice,
        setNotice
    ] =
        useState<string | null>(
            null
        );

    const selected =
        useMemo(
            () =>
                rows.find(
                    row =>
                        row.id ===
                        selectedId
                ) ?? null,
            [
                rows,
                selectedId
            ]
        );

    const chooseRow = (
        row: ExamSessionRow
    ) => {

        setSelectedId(
            row.id
        );

        setNewDate(
            row.date
        );

        setNewRoom(
            row.room
        );

        setNotice(null);
    };

    const saveChanges = () => {

        if (!selected) {
            return;
        }

        setRows(
            current =>
                current.map(
                    row =>
                        row.id ===
                        selected.id
                            ? {
                                ...row,

                                date:
                                newDate,

                                room:
                                newRoom,

                                status:
                                    "Ready"
                            }
                            : row
                )
        );

        setNotice(
            "UI updated locally. " +
            "Backend conflict validation " +
            "will be connected later."
        );
    };

    return (
        <PageShell
            title={
                "Edit current exam plan"
            }
            description={
                "Select a scheduled exam to review its date, room and current conflict status."
            }
        >

            <div
                className={
                    "split-editor"
                }
            >

                <section
                    className={
                        "panel-card table-panel"
                    }
                >

                    <div
                        className={
                            "panel-heading-row"
                        }
                    >

                        <div>
                            <h2>
                                Current schedule
                            </h2>

                            <p>
                                Version 3
                            </p>
                        </div>

                        <button
                            className={
                                "secondary-button"
                            }
                            type="button"
                            onClick={
                                () =>
                                    window.print()
                            }
                        >
                            Print view
                        </button>

                    </div>

                    <div
                        className={
                            "table-scroll"
                        }
                    >

                        <table
                            className={
                                "data-table"
                            }
                        >

                            <thead>
                            <tr>
                                <th>
                                    Exam
                                </th>

                                <th>
                                    Class
                                </th>

                                <th>
                                    Date
                                </th>

                                <th>
                                    Time
                                </th>

                                <th>
                                    Room
                                </th>

                                <th>
                                    Status
                                </th>

                                <th>
                                </th>
                            </tr>
                            </thead>

                            <tbody>

                            {
                                rows.map(
                                    row => (
                                        <tr
                                            key={
                                                row.id
                                            }
                                        >

                                            <td>
                                                {
                                                    row.course
                                                }
                                            </td>

                                            <td>
                                                {
                                                    row.classCode
                                                }
                                            </td>

                                            <td>
                                                {
                                                    row.date
                                                }
                                            </td>

                                            <td>
                                                {
                                                    row.time
                                                }
                                            </td>

                                            <td>
                                                {
                                                    row.room
                                                }
                                            </td>

                                            <td>

                                                    <span
                                                        className={
                                                            `status-badge status-${row.status.toLowerCase()}`
                                                        }
                                                    >
                                                        {
                                                            row.status
                                                        }
                                                    </span>

                                            </td>

                                            <td>

                                                <button
                                                    className={
                                                        "table-action"
                                                    }
                                                    type="button"
                                                    onClick={
                                                        () =>
                                                            chooseRow(
                                                                row
                                                            )
                                                    }
                                                >
                                                    Edit
                                                </button>

                                            </td>

                                        </tr>
                                    )
                                )
                            }

                            </tbody>

                        </table>

                    </div>

                </section>

                <aside
                    className={
                        "panel-card editor-card"
                    }
                >

                    <h2>
                        Edit session
                    </h2>

                    {!selected ? (

                        <p
                            className={
                                "muted-copy"
                            }
                        >
                            Select an exam
                            from the table.
                        </p>

                    ) : (

                        <>

                            <div
                                className={
                                    "selected-exam"
                                }
                            >

                                <strong>
                                    {
                                        selected.course
                                    }
                                </strong>

                                <span>
                                    {
                                        selected.classCode
                                    }
                                    {" · "}
                                    {
                                        selected.time
                                    }
                                </span>

                            </div>

                            <label
                                className={
                                    "form-field"
                                }
                            >

                                <span>
                                    Exam date
                                </span>

                                <input
                                    type="date"
                                    value={
                                        newDate
                                    }
                                    onChange={
                                        event =>
                                            setNewDate(
                                                event
                                                    .target
                                                    .value
                                            )
                                    }
                                />

                            </label>

                            <label
                                className={
                                    "form-field"
                                }
                            >

                                <span>
                                    Room
                                </span>

                                <input
                                    value={
                                        newRoom
                                    }
                                    onChange={
                                        event =>
                                            setNewRoom(
                                                event
                                                    .target
                                                    .value
                                            )
                                    }
                                />

                            </label>

                            <label
                                className={
                                    "checkbox-field"
                                }
                            >

                                <input
                                    type="checkbox"
                                    defaultChecked={
                                        selected.status ===
                                        "Locked"
                                    }
                                />

                                <span>
                                    Lock this
                                    exam date
                                </span>

                            </label>

                            <button
                                className={
                                    "primary-button full-width"
                                }
                                type="button"
                                onClick={
                                    saveChanges
                                }
                            >
                                Save change
                            </button>

                            {notice && (
                                <p
                                    className={
                                        "success-message"
                                    }
                                    role="status"
                                >
                                    {notice}
                                </p>
                            )}

                        </>

                    )}

                </aside>

            </div>

        </PageShell>
    );
};

export default EditPlan;