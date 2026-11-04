import {useEffect, useState} from "react";
import {useNavigate, useParams} from "react-router-dom";
import {useAuth} from "../api/features/auth/useAuth.ts";
import {getPlanningEducationElement, updatePlanningEducationElement} from "../api/features/planningElements/planningElementApi.ts";
import {getErrorMessage} from "../api/errorHandler.ts";
import type {PlanningEducationElement, UpdatePlanningEducationElementRequest} from "../api/features/planningElements/planningElementTypes.ts";
import viaLogo from "./Assets/via-logo-small.png";
import "./Themes/ImportedDataPage.css";

const toDraft = (
    element:
    PlanningEducationElement
): UpdatePlanningEducationElementRequest => ({
    name:
    element.name,
    nickname:
    element.nickname,
    ects:
    element.ects,
    executionElementCount:
    element.executionElementCount,
    startDate:
    element.startDate,
    endDate:
    element.endDate,
    activityOffering:
    element.activityOffering,
    prefix:
    element.prefix,
    assessmentType:
    element.assessmentType,
    completionAssessment:
    element.completionAssessment,
    gradingScale:
    element.gradingScale,
    combinedWrittenOral:
    element.combinedWrittenOral,
    oral:
    element.oral,
    practicalExam:
    element.practicalExam,
    project:
    element.project,
    written:
    element.written
});

const EditImportedDataPage = () => {

    const navigate =
        useNavigate();

    const {
        id
    } =
        useParams<{
            id: string;
        }>();

    const {
        user
    } =
        useAuth();

    const [
        element,
        setElement
    ] =
        useState<
            PlanningEducationElement | null
        >(null);

    const [
        draft,
        setDraft
    ] =
        useState<
            UpdatePlanningEducationElementRequest
            | null
        >(null);

    const [
        loading,
        setLoading
    ] =
        useState(true);

    const [
        saving,
        setSaving
    ] =
        useState(false);

    const [
        error,
        setError
    ] =
        useState<string | null>(
            null
        );

    const [
        success,
        setSuccess
    ] =
        useState<string | null>(
            null
        );

    const isAdmin =
        user?.role?.toLowerCase() ===
        "admin";

    useEffect(
        () => {
            if (!id)
            {
                return;
            }
            let cancelled =
                false;
            getPlanningEducationElement(
                id
            )
                .then(
                    result => {
                        if (cancelled)
                        {
                            return;
                        }
                        setElement(
                            result
                        );
                        setDraft(
                            toDraft(
                                result
                            )
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
        [id]
    );

    const updateField =
        <
            K extends keyof
                UpdatePlanningEducationElementRequest
        >(
            key: K,
            value:
            UpdatePlanningEducationElementRequest[K]
        ) => {
            setDraft(
                current =>
                    current
                        ? {
                            ...current,
                            [key]: value
                        }
                        : current
            );
        };

    const save =
        async () => {
            if (
                !id ||
                !draft ||
                !isAdmin
            )
            {
                return;
            }
            if (!draft.name.trim())
            {
                setError(
                    "Name must not be empty."
                );
                return;
            }
            if (
                !draft.nickname.trim()
            )
            {
                setError(
                    "Nickname must not be empty."
                );
                return;
            }
            if (
                draft.ects < 0 ||
                draft.executionElementCount < 0
            )
            {
                setError(
                    "ECTS and execution-element count must be zero or greater."
                );
                return;
            }
            if (
                draft.endDate <
                draft.startDate
            )
            {
                setError(
                    "End date must be on or after start date."
                );
                return;
            }
            setSaving(true);
            setError(null);
            setSuccess(null);
            try {
                const updated =
                    await updatePlanningEducationElement(
                        id,
                        {
                            ...draft,
                            name:
                                draft.name.trim(),
                            nickname:
                                draft.nickname.trim(),
                            activityOffering:
                                draft.activityOffering.trim(),
                            prefix:
                                draft.prefix.trim(),
                            gradingScale:
                                draft.gradingScale.trim()
                        }
                    );
                setElement(
                    updated
                );
                setDraft(
                    toDraft(
                        updated
                    )
                );
                setSuccess(
                    "Changes were saved to the database."
                );
            } catch (saveError) {
                setError(
                    getErrorMessage(
                        saveError
                    )
                );
            } finally {
                setSaving(false);
            }
        };

    if (!isAdmin)
    {
        return (
            <main className="imported-data-page">
                <div className="imported-data-title">
                    <div>
                        <h1>
                            Edit imported data
                        </h1>
                        <p>
                            You must be an
                            administrator to edit
                            imported data.
                        </p>
                    </div>
                </div>
                <section className="imported-data-layout">
                    <div className="imported-data-card imported-state">
                        <button
                            type="button"
                            className="imported-primary-button"
                            onClick={
                                () =>
                                    navigate(
                                        "/imported-data"
                                    )
                            }
                        >
                            Back to imported data
                        </button>
                    </div>
                </section>
            </main>
        );
    }

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
                <button
                    type="button"
                    className="imported-secondary-button"
                    onClick={
                        () =>
                            navigate(
                                "/imported-data"
                            )
                    }
                >
                    Back to imported data
                </button>
            </header>
            <div className="imported-data-title">
                <div>
                    <p className="imported-data-eyebrow">
                        Edit imported data
                    </p>
                    <h1>
                        {
                            element?.nickname ||
                            "Planning education element"
                        }
                    </h1>
                    <p>
                        Source ID, checksum and
                        source-modified time are
                        read-only.
                    </p>
                </div>
            </div>
            <section className="imported-data-layout">
                <div className="imported-data-card">
                    {loading && (
                        <div className="imported-state">
                            Loading record...
                        </div>
                    )}
                    {!loading &&
                        error &&
                        !draft && (
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
                                            navigate(
                                                "/imported-data"
                                            )
                                    }
                                >
                                    Back to imported data
                                </button>
                            </div>
                        )}
                    {!loading &&
                        element &&
                        draft && (
                            <form
                                onSubmit={
                                    event => {
                                        event.preventDefault();
                                        void save();
                                    }
                                }
                            >
                                <div className="imported-source-grid">
                                    <div>
                                    <span>
                                        Source ID
                                    </span>
                                        <strong>
                                            {
                                                element.id
                                            }
                                        </strong>
                                    </div>
                                    <div>
                                    <span>
                                        Source modified
                                    </span>
                                        <strong>
                                            {
                                                new Date(
                                                    element.sourceModifiedAt
                                                )
                                                    .toLocaleString()
                                            }
                                        </strong>
                                    </div>
                                    <div>
                                    <span>
                                        Source checksum
                                    </span>
                                        <strong className="imported-code-value">
                                            {
                                                element.sourceChecksum
                                            }
                                        </strong>
                                    </div>
                                </div>
                                <div className="imported-form-grid">
                                    <label className="imported-field">
                                    <span>
                                        Name
                                    </span>
                                        <input
                                            value={
                                                draft.name
                                            }
                                            onChange={
                                                event =>
                                                    updateField(
                                                        "name",
                                                        event.target.value
                                                    )
                                            }
                                            disabled={
                                                saving
                                            }
                                        />
                                    </label>
                                    <label className="imported-field">
                                    <span>
                                        Nickname
                                    </span>
                                        <input
                                            value={
                                                draft.nickname
                                            }
                                            onChange={
                                                event =>
                                                    updateField(
                                                        "nickname",
                                                        event.target.value
                                                    )
                                            }
                                            disabled={
                                                saving
                                            }
                                        />
                                    </label>
                                    <label className="imported-field">
                                    <span>
                                        Prefix
                                    </span>
                                        <input
                                            value={
                                                draft.prefix
                                            }
                                            onChange={
                                                event =>
                                                    updateField(
                                                        "prefix",
                                                        event.target.value
                                                    )
                                            }
                                            disabled={
                                                saving
                                            }
                                        />
                                    </label>
                                    <label className="imported-field">
                                    <span>
                                        ECTS
                                    </span>
                                        <input
                                            type="number"
                                            min="0"
                                            value={
                                                draft.ects
                                            }
                                            onChange={
                                                event =>
                                                    updateField(
                                                        "ects",
                                                        Number(
                                                            event.target.value
                                                        )
                                                    )
                                            }
                                            disabled={
                                                saving
                                            }
                                        />
                                    </label>
                                    <label className="imported-field">
                                    <span>
                                        Execution element count
                                    </span>
                                        <input
                                            type="number"
                                            min="0"
                                            value={
                                                draft.executionElementCount
                                            }
                                            onChange={
                                                event =>
                                                    updateField(
                                                        "executionElementCount",
                                                        Number(
                                                            event.target.value
                                                        )
                                                    )
                                            }
                                            disabled={
                                                saving
                                            }
                                        />
                                    </label>
                                    <label className="imported-field">
                                    <span>
                                        Assessment type
                                    </span>
                                        <select
                                            value={
                                                draft.assessmentType
                                            }
                                            onChange={
                                                event => {
                                                    const value =
                                                        event.target.value;
                                                    updateField(
                                                        "assessmentType",
                                                        value === "External"
                                                            ? "External"
                                                            : "Internal"
                                                    );
                                                }
                                            }
                                            disabled={
                                                saving
                                            }
                                        >
                                            <option value="Internal">
                                                Internal
                                            </option>
                                            <option value="External">
                                                External
                                            </option>
                                        </select>
                                    </label>
                                    <label className="imported-field">
                                    <span>
                                        Start date
                                    </span>
                                        <input
                                            type="date"
                                            value={
                                                draft.startDate
                                            }
                                            onChange={
                                                event =>
                                                    updateField(
                                                        "startDate",
                                                        event.target.value
                                                    )
                                            }
                                            disabled={
                                                saving
                                            }
                                        />
                                    </label>
                                    <label className="imported-field">
                                    <span>
                                        End date
                                    </span>
                                        <input
                                            type="date"
                                            value={
                                                draft.endDate
                                            }
                                            onChange={
                                                event =>
                                                    updateField(
                                                        "endDate",
                                                        event.target.value
                                                    )
                                            }
                                            disabled={
                                                saving
                                            }
                                        />
                                    </label>
                                </div>
                                <label className="imported-field">
                                <span>
                                    Activity offering
                                </span>
                                    <textarea
                                        rows={3}
                                        value={
                                            draft.activityOffering
                                        }
                                        onChange={
                                            event =>
                                                updateField(
                                                    "activityOffering",
                                                    event.target.value
                                                )
                                        }
                                        disabled={
                                            saving
                                        }
                                    />
                                </label>
                                <label className="imported-field">
                                <span>
                                    Grading scale
                                </span>
                                    <input
                                        value={
                                            draft.gradingScale
                                        }
                                        onChange={
                                            event =>
                                                updateField(
                                                    "gradingScale",
                                                    event.target.value
                                                )
                                        }
                                        disabled={
                                            saving
                                        }
                                    />
                                </label>
                                <fieldset
                                    className="imported-checkbox-group"
                                    disabled={
                                        saving
                                    }
                                >
                                    <legend>
                                        Assessment forms
                                    </legend>
                                    {([
                                        [
                                            "completionAssessment",
                                            "Completion"
                                        ],
                                        [
                                            "combinedWrittenOral",
                                            "Combined written/oral"
                                        ],
                                        [
                                            "oral",
                                            "Oral"
                                        ],
                                        [
                                            "practicalExam",
                                            "Practical exam"
                                        ],
                                        [
                                            "project",
                                            "Project"
                                        ],
                                        [
                                            "written",
                                            "Written"
                                        ]
                                    ] as const)
                                        .map(
                                            (
                                                [
                                                    key,
                                                    label
                                                ]
                                            ) => (
                                                <label
                                                    key={
                                                        key
                                                    }
                                                >
                                                    <input
                                                        type="checkbox"
                                                        checked={
                                                            draft[
                                                                key
                                                                ]
                                                        }
                                                        onChange={
                                                            event =>
                                                                updateField(
                                                                    key,
                                                                    event.target.checked
                                                                )
                                                        }
                                                    />
                                                    <span>
                                                    {
                                                        label
                                                    }
                                                </span>
                                                </label>
                                            )
                                        )
                                    }
                                </fieldset>
                                {error && (
                                    <p
                                        className="imported-error"
                                        role="alert"
                                    >
                                        {error}
                                    </p>
                                )}
                                {success && (
                                    <p
                                        className="imported-success"
                                        role="status"
                                    >
                                        {success}
                                    </p>
                                )}
                                <div className="imported-editor-actions">
                                    <button
                                        type="button"
                                        className="imported-secondary-button"
                                        onClick={
                                            () =>
                                                navigate(
                                                    "/imported-data"
                                                )
                                        }
                                        disabled={
                                            saving
                                        }
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="submit"
                                        className="imported-primary-button"
                                        disabled={
                                            saving
                                        }
                                    >
                                        {
                                            saving
                                                ? "Saving..."
                                                : "Save changes"
                                        }
                                    </button>
                                </div>
                            </form>
                        )}
                </div>
            </section>
        </main>
    );
};

export default EditImportedDataPage;