import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
import {useAuth} from "../api/features/auth/useAuth.ts";
import {getPlanningEducationElements} from "../api/features/planningElements/planningElementApi.ts";
import {getErrorMessage} from "../api/errorHandler.ts";
import type {PlanningEducationElement} from "../api/features/planningElements/planningElementTypes.ts";
import viaLogo from "./Assets/via-logo-small.png";
import "./Themes/ImportedDataPage.css";

const assessmentForms = (
    element: PlanningEducationElement
): string => {

    const forms: string[] = [];

    if (
        element.completionAssessment
    )
    {
        forms.push(
            "Completion"
        );
    }

    if (
        element.combinedWrittenOral
    )
    {
        forms.push(
            "Combined written/oral"
        );
    }

    if (element.oral)
    {
        forms.push(
            "Oral"
        );
    }

    if (
        element.practicalExam
    )
    {
        forms.push(
            "Practical"
        );
    }

    if (element.project)
    {
        forms.push(
            "Project"
        );
    }

    if (element.written)
    {
        forms.push(
            "Written"
        );
    }

    return forms.length > 0
        ? forms.join(", ")
        : "—";
};

const ImportedDataPage = () => {
    const navigate =
        useNavigate();

    const {
        user
    } =
        useAuth();

    const [
        elements,
        setElements
    ] =
        useState<
            PlanningEducationElement[]
        >([]);

    const [
        loading,
        setLoading
    ] =
        useState(true);

    const [
        error,
        setError
    ] =
        useState<string | null>(
            null
        );

    const isAdmin =
        user?.role?.toLowerCase() ===
        "admin";

    useEffect(
        () => {
            let cancelled =
                false;
            getPlanningEducationElements()
                .then(
                    result => {
                        if (cancelled)
                        {
                            return;
                        }
                        setElements(
                            result
                        );
                        setError(null);
                    }
                )
                .catch(
                    loadError => {
                        if (cancelled)
                        {
                            return;
                        }
                        setError(
                            getErrorMessage(
                                loadError
                            )
                        );
                    }
                )
                .finally(
                    () => {
                        if (!cancelled)
                        {
                            setLoading(
                                false
                            );
                        }
                    }
                );
            return () => {
                cancelled =
                    true;
            };
        },
        []
    );

    const retry =
        async () => {
            setLoading(true);
            setError(null);

            try {
                const result =
                    await getPlanningEducationElements();

                setElements(
                    result
                );
            } catch (loadError) {
                setError(
                    getErrorMessage(
                        loadError
                    )
                );
            } finally {
                setLoading(false);
            }
        };

    return (
        <main className="imported-data-page">
            <header className="imported-data-topbar">
                <button
                    type="button"
                    className="imported-data-brand"
                    onClick={
                        () =>
                            navigate(
                                "/Main"
                            )
                    }
                >
                    <img
                        src={viaLogo}
                        alt="VIA University College"
                    />
                    <span>
                        Exam Plan
                    </span>
                </button>
                <div className="imported-data-topbar-right">
                    {user && (
                        <span className="imported-data-user">
                            {
                                user.name ||
                                user.email
                            }
                        </span>
                    )}
                    <button
                        type="button"
                        className="imported-secondary-button"
                        onClick={
                            () =>
                                navigate(
                                    "/Main"
                                )
                        }
                    >
                        Back to main page
                    </button>
                </div>
            </header>
            <div className="imported-data-title">
                <div>
                    <p className="imported-data-eyebrow">
                        Imported data
                    </p>
                    <h1>
                        Planning education elements
                    </h1>
                    <p>
                        These are the records
                        created by the Excel import.
                        Administrators can edit the
                        stored planning data.
                    </p>
                </div>
                {!loading &&
                    !error && (
                        <span className="imported-data-count">
                        {
                            elements.length
                        }
                            {" "}
                            record
                            {
                                elements.length === 1
                                    ? ""
                                    : "s"
                            }
                    </span>
                    )}
            </div>
            <section className="imported-data-layout">
                <div className="imported-data-card">
                    {!isAdmin && (
                        <p className="imported-info">
                            You can view the
                            imported data.
                            Editing is available
                            to administrators.
                        </p>
                    )}
                    {loading && (
                        <div className="imported-state">
                            Loading imported data...
                        </div>
                    )}
                    {!loading &&
                        error && (
                            <div className="imported-state">
                                <p
                                    className="imported-error"
                                    role="alert"
                                >
                                    {error}
                                </p>
                                <button
                                    type="button"
                                    className="imported-primary-button"
                                    onClick={
                                        () =>
                                            void retry()
                                    }
                                >
                                    Try again
                                </button>
                            </div>
                        )}
                    {!loading &&
                        !error &&
                        elements.length === 0 && (
                            <div className="imported-state">
                                <h2>
                                    No imported data found
                                </h2>
                                <p>
                                    Import the planning
                                    element spreadsheet
                                    first.
                                </p>
                                <button
                                    type="button"
                                    className="imported-primary-button"
                                    onClick={
                                        () =>
                                            navigate(
                                                "/import"
                                            )
                                    }
                                >
                                    Go to Import Data
                                </button>
                            </div>
                        )}
                    {!loading &&
                        !error &&
                        elements.length > 0 && (
                            <div className="imported-table-wrap">
                                <table className="imported-table">
                                    <thead>
                                    <tr>
                                        <th>
                                            Prefix
                                        </th>
                                        <th>
                                            Nickname
                                        </th>
                                        <th>
                                            Name
                                        </th>
                                        <th>
                                            ECTS
                                        </th>
                                        <th>
                                            Start
                                        </th>
                                        <th>
                                            End
                                        </th>
                                        <th>
                                            Assessment
                                        </th>
                                        <th>
                                            Forms
                                        </th>
                                        {isAdmin && (
                                            <th>
                                                Action
                                            </th>
                                        )}
                                    </tr>
                                    </thead>
                                    <tbody>
                                    {
                                        elements.map(
                                            element => (
                                                <tr
                                                    key={
                                                        element.id
                                                    }
                                                >
                                                    <td>
                                                        {
                                                            element.prefix ||
                                                            "—"
                                                        }
                                                    </td>
                                                    <td>
                                                        {
                                                            element.nickname
                                                        }
                                                    </td>
                                                    <td>
                                                        {
                                                            element.name
                                                        }
                                                    </td>
                                                    <td>
                                                        {
                                                            element.ects
                                                        }
                                                    </td>
                                                    <td>
                                                        {
                                                            element.startDate
                                                        }
                                                    </td>
                                                    <td>
                                                        {
                                                            element.endDate
                                                        }
                                                    </td>
                                                    <td>
                                                        {
                                                            element.assessmentType
                                                        }
                                                    </td>
                                                    <td>
                                                        {
                                                            assessmentForms(
                                                                element
                                                            )
                                                        }
                                                    </td>
                                                    {isAdmin && (
                                                        <td>
                                                            <button
                                                                type="button"
                                                                className="imported-edit-button"
                                                                onClick={
                                                                    () =>
                                                                        navigate(
                                                                            `/imported-data/${element.id}/edit`
                                                                        )
                                                                }
                                                            >
                                                                Edit
                                                            </button>
                                                        </td>
                                                    )}
                                                </tr>
                                            )
                                        )
                                    }
                                    </tbody>
                                </table>
                            </div>
                        )}
                </div>
            </section>
        </main>
    );
};

export default ImportedDataPage;