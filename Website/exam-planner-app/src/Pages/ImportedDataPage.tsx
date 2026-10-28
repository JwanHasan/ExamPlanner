import {
    useEffect,
    useState
} from "react";

import {
    useNavigate
} from "react-router-dom";

import {
    useAuth
} from "../api/features/auth/AuthContext.tsx";

import {
    getCourses,
    updateCourse
} from "../api/features/courses/courseApi.ts";

import type {
    Course
} from "../api/features/courses/courseTypes.ts";

import {
    getErrorMessage
} from "../api/errorHandler.ts";

import viaLogo
    from "./Assets/via-logo-small.png";

import "./Themes/ImportedDataPage.css";

const ImportedDataPage = () => {
    const navigate =
        useNavigate();

    const {
        user
    } =
        useAuth();

    const [
        courses,
        setCourses
    ] =
        useState<Course[]>([]);

    const [
        loading,
        setLoading
    ] =
        useState(true);

    const [
        loadError,
        setLoadError
    ] =
        useState<string | null>(
            null
        );

    const [
        selectedCourse,
        setSelectedCourse
    ] =
        useState<Course | null>(
            null
        );

    const [
        editName,
        setEditName
    ] =
        useState("");

    const [
        editSueCode,
        setEditSueCode
    ] =
        useState("");

    const [
        saving,
        setSaving
    ] =
        useState(false);

    const [
        saveError,
        setSaveError
    ] =
        useState<string | null>(
            null
        );

    const [
        successMessage,
        setSuccessMessage
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

            getCourses()
                .then(
                    result => {

                        if (cancelled) {
                            return;
                        }

                        setCourses(result);

                        setLoadError(null);
                    }
                )
                .catch(
                    error => {

                        if (cancelled) {
                            return;
                        }

                        setLoadError(
                            getErrorMessage(
                                error
                            )
                        );
                    }
                )
                .finally(
                    () => {

                        if (cancelled) {
                            return;
                        }

                        setLoading(false);
                    }
                );

            return () => {

                cancelled =
                    true;
            };
        },
        []
    );

    const retryLoadCourses =
        async () => {

            setLoading(true);

            setLoadError(null);

            setSuccessMessage(null);

            try {

                const result =
                    await getCourses();

                setCourses(result);

            } catch (error) {

                setLoadError(
                    getErrorMessage(
                        error
                    )
                );

            } finally {

                setLoading(false);
            }
        };

    const beginEdit = (
        course: Course
    ) => {

        setSelectedCourse(
            course
        );

        setEditName(
            course.name
        );

        setEditSueCode(
            course.sueCode
        );

        setSaveError(null);

        setSuccessMessage(null);
    };

    const cancelEdit = () => {

        setSelectedCourse(null);

        setEditName("");

        setEditSueCode("");

        setSaveError(null);
    };

    const saveCourse =
        async () => {

            if (!selectedCourse) {
                return;
            }

            const name =
                editName.trim();

            const sueCode =
                editSueCode.trim();

            if (!name) {

                setSaveError(
                    "Course name must not be empty."
                );

                return;
            }

            if (!sueCode) {

                setSaveError(
                    "SUE code must not be empty."
                );

                return;
            }


            setSaving(true);

            setSaveError(null);

            setSuccessMessage(null);


            try {

                const updated =
                    await updateCourse(
                        selectedCourse.id,
                        {
                            name,
                            sueCode,

                            notes:
                            selectedCourse.notes,

                            classCount:
                            selectedCourse.classCount
                        }
                    );

                setCourses(
                    current =>
                        current.map(
                            course =>
                                course.id ===
                                updated.id
                                    ? updated
                                    : course
                        )
                );

                setSelectedCourse(
                    null
                );

                setEditName("");

                setEditSueCode("");

                setSuccessMessage(
                    `Course "${updated.sueCode}" was updated successfully.`
                );

            } catch (error) {

                setSaveError(
                    getErrorMessage(
                        error
                    )
                );

            } finally {

                setSaving(false);
            }
        };

    return (
        <main
            className="imported-data-page"
        >

            <header
                className="imported-data-topbar"
            >

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

                <div
                    className="imported-data-topbar-right"
                >

                    {user && (

                        <span
                            className="imported-data-user"
                        >
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

            <div
                className="imported-data-title"
            >

                <div>

                    <p
                        className="imported-data-eyebrow"
                    >
                        Imported data
                    </p>

                    <h1>
                        Courses
                    </h1>

                    <p>
                        View courses that have
                        been imported into the
                        Exam Planner database.
                    </p>

                </div>

                {!loading &&
                    !loadError && (

                        <span
                            className="imported-data-count"
                        >
                        {
                            courses.length
                        }
                            {" "}
                            course
                            {
                                courses.length === 1
                                    ? ""
                                    : "s"
                            }
                    </span>

                    )}

            </div>

            <section
                className={
                    `imported-data-layout ${
                        selectedCourse
                            ? "has-editor"
                            : ""
                    }`
                }
            >

                <div
                    className="imported-data-card"
                >

                    {successMessage && (

                        <p
                            className="imported-success"
                            role="status"
                        >
                            {
                                successMessage
                            }
                        </p>

                    )}

                    {!isAdmin && (

                        <p
                            className="imported-info"
                        >
                            You can view imported
                            courses. Editing is
                            available to
                            administrators.
                        </p>

                    )}

                    {loading && (

                        <p
                            className="imported-state"
                        >
                            Loading imported data...
                        </p>

                    )}

                    {!loading &&
                        loadError && (

                            <div
                                className="imported-state"
                            >

                                <p
                                    className="imported-error"
                                    role="alert"
                                >
                                    {
                                        loadError
                                    }
                                </p>


                                <button
                                    type="button"
                                    className="imported-primary-button"
                                    onClick={
                                        () =>
                                            void retryLoadCourses()
                                    }
                                >
                                    Try again
                                </button>

                            </div>

                        )}

                    {!loading &&
                        !loadError &&
                        courses.length === 0 && (

                            <div
                                className="imported-state"
                            >

                                <h2>
                                    No imported courses
                                </h2>

                                <p>
                                    Import data before
                                    attempting to view
                                    or edit courses.
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
                        !loadError &&
                        courses.length > 0 && (

                            <div
                                className="imported-table-wrap"
                            >

                                <table
                                    className="imported-table"
                                >

                                    <thead>

                                    <tr>

                                        <th>
                                            SUE Code
                                        </th>

                                        <th>
                                            Name
                                        </th>

                                        <th>
                                            Semester
                                        </th>

                                        <th>
                                            ECTS
                                        </th>

                                        <th>
                                            Classes
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
                                        courses.map(
                                            course => (

                                                <tr
                                                    key={
                                                        course.id
                                                    }
                                                >

                                                    <td>
                                                        {
                                                            course.sueCode
                                                        }
                                                    </td>

                                                    <td>
                                                        {
                                                            course.name
                                                        }
                                                    </td>

                                                    <td>
                                                        {
                                                            course.semester
                                                        }
                                                    </td>

                                                    <td>
                                                        {
                                                            course.ects
                                                        }
                                                    </td>

                                                    <td>
                                                        {
                                                            course.classCount
                                                        }
                                                    </td>

                                                    {isAdmin && (

                                                        <td>

                                                            <button
                                                                type="button"
                                                                className="imported-edit-button"
                                                                onClick={
                                                                    () =>
                                                                        beginEdit(
                                                                            course
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

                {selectedCourse &&
                    isAdmin && (

                        <aside
                            className={
                                "imported-data-card imported-editor"
                            }
                        >

                            <h2>
                                Edit Course
                            </h2>

                            <form
                                onSubmit={
                                    event => {

                                        event.preventDefault();

                                        void saveCourse();
                                    }
                                }
                            >

                                <label
                                    className="imported-field"
                                >

                                <span>
                                    SUE Code
                                </span>

                                    <input
                                        type="text"
                                        value={
                                            editSueCode
                                        }
                                        onChange={
                                            event =>
                                                setEditSueCode(
                                                    event
                                                        .target
                                                        .value
                                                )
                                        }
                                        disabled={
                                            saving
                                        }
                                    />

                                </label>

                                <label
                                    className="imported-field"
                                >

                                <span>
                                    Name
                                </span>

                                    <input
                                        type="text"
                                        value={
                                            editName
                                        }
                                        onChange={
                                            event =>
                                                setEditName(
                                                    event
                                                        .target
                                                        .value
                                                )
                                        }
                                        disabled={
                                            saving
                                        }
                                    />

                                </label>

                                <div
                                    className="imported-readonly-grid"
                                >

                                    <div>

                                    <span>
                                        Semester
                                    </span>

                                        <strong>
                                            {
                                                selectedCourse.semester
                                            }
                                        </strong>

                                    </div>

                                    <div>

                                    <span>
                                        ECTS
                                    </span>

                                        <strong>
                                            {
                                                selectedCourse.ects
                                            }
                                        </strong>

                                    </div>

                                    <div>

                                    <span>
                                        Classes
                                    </span>

                                        <strong>
                                            {
                                                selectedCourse.classCount
                                            }
                                        </strong>

                                    </div>

                                </div>

                                {saveError && (

                                    <p
                                        className="imported-error"
                                        role="alert"
                                    >
                                        {
                                            saveError
                                        }
                                    </p>

                                )}

                                <div
                                    className="imported-editor-actions"
                                >

                                    <button
                                        type="button"
                                        className="imported-secondary-button"
                                        onClick={
                                            cancelEdit
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

                        </aside>

                    )}

            </section>

        </main>
    );
};

export default ImportedDataPage;