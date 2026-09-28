import PageShell
    from "../components/PageShell";

import {
    issues
} from "../data/mockData";

import "./Themes/Plan.css";

const Issues = () => (
    <PageShell
        title="Plan issues"
        description={
            "Review detected student, lecturer and room conflicts before publishing the schedule."
        }
    >

        <section
            className="panel-card"
        >

            <div
                className={
                    "panel-heading-row"
                }
            >

                <div>

                    <h2>
                        Open issues
                    </h2>

                    <p>
                        {issues.length}
                        {" "}
                        issue(s) require
                        attention
                    </p>

                </div>

                <button
                    className={
                        "primary-button"
                    }
                    type="button"
                    onClick={
                        () =>
                            window.alert(
                                "Conflict scan endpoint is not implemented yet."
                            )
                    }
                >
                    Scan again
                </button>

            </div>

            <div
                className={
                    "issue-list"
                }
            >

                {
                    issues.map(
                        issue => (

                            <article
                                className={
                                    "issue-card"
                                }
                                key={
                                    issue.id
                                }
                            >

                                <div
                                    className={
                                        "issue-type"
                                    }
                                >
                                    {
                                        issue.type
                                    }
                                </div>

                                <div
                                    className={
                                        "issue-main"
                                    }
                                >

                                    <strong>
                                        {
                                            issue.title
                                        }
                                    </strong>

                                    <span>
                                        {
                                            issue.details
                                        }
                                    </span>

                                    <small>
                                        {
                                            issue.date
                                        }
                                    </small>

                                </div>

                                <div
                                    className={
                                        "issue-count"
                                    }
                                >

                                    <strong>
                                        {
                                            issue.affected
                                        }
                                    </strong>

                                    <span>
                                        affected
                                    </span>

                                </div>

                            </article>

                        )
                    )
                }

            </div>

        </section>

    </PageShell>
);

export default Issues;