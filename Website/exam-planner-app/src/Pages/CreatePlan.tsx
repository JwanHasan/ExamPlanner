import {
    useMemo,
    useState
} from "react";

import PageShell
    from "../components/PageShell";

import "./Themes/Plan.css";

const CreatePlan = () => {

    const [
        name,
        setName
    ] =
        useState(
            "Winter exams 2027"
        );

    const [
        startDate,
        setStartDate
    ] =
        useState(
            "2027-01-04"
        );

    const [
        endDate,
        setEndDate
    ] =
        useState(
            "2027-01-29"
        );

    const [
        ordinaryDays,
        setOrdinaryDays
    ] =
        useState(
            "2027-01-11\n" +
            "2027-01-12\n" +
            "2027-01-13\n" +
            "2027-01-14\n" +
            "2027-01-15"
        );

    const [
        reExamDays,
        setReExamDays
    ] =
        useState(
            "2027-02-15\n" +
            "2027-02-16"
        );

    const [
        message,
        setMessage
    ] =
        useState<string | null>(
            null
        );

    const ordinaryCount =
        useMemo(
            () =>
                ordinaryDays
                    .split("\n")
                    .filter(Boolean)
                    .length,
            [ordinaryDays]
        );

    const reExamCount =
        useMemo(
            () =>
                reExamDays
                    .split("\n")
                    .filter(Boolean)
                    .length,
            [reExamDays]
        );

    const handleGenerate = () => {

        setMessage(
            `Draft UI prepared with ` +
            `${ordinaryCount} ordinary day(s) ` +
            `and ${reExamCount} re-exam day(s). ` +
            `Scheduling logic will be connected ` +
            `to the backend later.`
        );
    };

    return (
        <PageShell
            title="Create exam plan"
            description={
                "Define the planning period and the exam days that the scheduler may use."
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
                        Plan details
                    </h2>

                    <div
                        className="form-grid"
                    >

                        <label
                            className={
                                "form-field full-span"
                            }
                        >

                            <span>
                                Plan name
                            </span>

                            <input
                                value={name}
                                onChange={
                                    event =>
                                        setName(
                                            event
                                                .target
                                                .value
                                        )
                                }
                            />

                        </label>

                        <label
                            className="form-field"
                        >

                            <span>
                                Start date
                            </span>

                            <input
                                type="date"
                                value={
                                    startDate
                                }
                                onChange={
                                    event =>
                                        setStartDate(
                                            event
                                                .target
                                                .value
                                        )
                                }
                            />

                        </label>

                        <label
                            className="form-field"
                        >

                            <span>
                                End date
                            </span>

                            <input
                                type="date"
                                value={
                                    endDate
                                }
                                onChange={
                                    event =>
                                        setEndDate(
                                            event
                                                .target
                                                .value
                                        )
                                }
                            />

                        </label>

                    </div>

                    <h2
                        className={
                            "section-title"
                        }
                    >
                        Exam days
                    </h2>

                    <div
                        className="form-grid"
                    >

                        <label
                            className="form-field"
                        >

                            <span>
                                Ordinary exam days
                            </span>

                            <textarea
                                rows={8}
                                value={
                                    ordinaryDays
                                }
                                onChange={
                                    event =>
                                        setOrdinaryDays(
                                            event
                                                .target
                                                .value
                                        )
                                }
                            />

                            <small>
                                One date per line.
                            </small>

                        </label>

                        <label
                            className="form-field"
                        >

                            <span>
                                Re-exam days
                            </span>

                            <textarea
                                rows={8}
                                value={
                                    reExamDays
                                }
                                onChange={
                                    event =>
                                        setReExamDays(
                                            event
                                                .target
                                                .value
                                        )
                                }
                            />

                            <small>
                                One date per line.
                            </small>

                        </label>

                    </div>

                </section>

                <aside
                    className={
                        "panel-card summary-card"
                    }
                >

                    <p
                        className="eyebrow"
                    >
                        Draft summary
                    </p>

                    <h2>
                        {
                            name ||
                            "Unnamed plan"
                        }
                    </h2>

                    <dl
                        className={
                            "summary-list"
                        }
                    >

                        <div>
                            <dt>
                                Period
                            </dt>

                            <dd>
                                {startDate}
                                {" → "}
                                {endDate}
                            </dd>
                        </div>

                        <div>
                            <dt>
                                Ordinary days
                            </dt>

                            <dd>
                                {
                                    ordinaryCount
                                }
                            </dd>
                        </div>

                        <div>
                            <dt>
                                Re-exam days
                            </dt>

                            <dd>
                                {
                                    reExamCount
                                }
                            </dd>
                        </div>

                        <div>
                            <dt>
                                Status
                            </dt>

                            <dd>
                                Not generated
                            </dd>
                        </div>

                    </dl>

                    <div
                        className="rule-box"
                    >

                        <strong>
                            Generation will
                            later apply
                        </strong>

                        <span>
                            Student overlaps
                        </span>

                        <span>
                            Lecturer constraints
                        </span>

                        <span>
                            Priority ordering
                        </span>

                        <span>
                            Room availability
                            and capacity
                        </span>

                    </div>

                    <button
                        className={
                            "primary-button full-width"
                        }
                        type="button"
                        onClick={
                            handleGenerate
                        }
                    >
                        Generate draft plan
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

                </aside>

            </div>

        </PageShell>
    );
};

export default CreatePlan;