import {
    useNavigate
} from "react-router-dom";

import {
    useAuth
} from "../auth/AuthContext";

import ActionCard
    from "../components/ActionCard";

import AppHeader
    from "../components/AppHeader";

import campusBackground
    from "./Assets/campus-background-transparent.png";

import "./Themes/Main.css";

const Main = () => {

    const {
        user
    } =
        useAuth();

    const navigate =
        useNavigate();

    return (
        <main
            className="dashboard"
            style={{
                backgroundImage:
                    `url(${campusBackground})`
            }}
        >

            <AppHeader
                showBack={false}
            />

            <div
                className={
                    "dashboard-content"
                }
            >

                <section
                    className={
                        "dashboard-intro"
                    }
                >

                    <p
                        className="eyebrow"
                    >
                        Exam Planner
                    </p>

                    <h1>
                        Welcome, {
                        user?.name ||
                        "Admin"
                    }
                    </h1>

                    <p>
                        Build, review and
                        publish the current
                        exam plan.
                    </p>

                </section>

                <section
                    className={
                        "dashboard-summary"
                    }
                    aria-label={
                        "Current plan summary"
                    }
                >

                    <div>
                        <strong>
                            42
                        </strong>

                        <span>
                            planned exams
                        </span>
                    </div>

                    <div>
                        <strong>
                            3
                        </strong>

                        <span>
                            open issues
                        </span>
                    </div>

                    <div>
                        <strong>
                            2
                        </strong>

                        <span>
                            teacher reviews
                            pending
                        </span>
                    </div>

                    <div>
                        <strong>
                            v3
                        </strong>

                        <span>
                            current version
                        </span>
                    </div>

                </section>

                <section
                    className="action-grid"
                    aria-label={
                        "Exam planning actions"
                    }
                >

                    <ActionCard
                        title={
                            "Create new exam plan"
                        }
                        description={
                            "Choose exam days and generate a new draft schedule."
                        }
                        onClick={
                            () =>
                                navigate(
                                    "/create-plan"
                                )
                        }
                    />

                    <ActionCard
                        title={
                            "Edit current exam plan"
                        }
                        description={
                            "Review planned sessions and move individual exams."
                        }
                        onClick={
                            () =>
                                navigate(
                                    "/edit-plan"
                                )
                        }
                    />

                    <ActionCard
                        title={
                            "Scan for issues"
                        }
                        description={
                            "Check student, lecturer and room conflicts."
                        }
                        emphasis={
                            "warning"
                        }
                        onClick={
                            () =>
                                navigate(
                                    "/issues"
                                )
                        }
                    />

                    <ActionCard
                        title={
                            "Import data"
                        }
                        description={
                            "Select the Excel source data used by the planner."
                        }
                        onClick={
                            () =>
                                navigate(
                                    "/import"
                                )
                        }
                    />

                    <ActionCard
                        title={
                            "Manage rooms"
                        }
                        description={
                            "Review room capacity and campus information."
                        }
                        onClick={
                            () =>
                                navigate(
                                    "/rooms"
                                )
                        }
                    />

                    <ActionCard
                        title={
                            "Teacher reviews"
                        }
                        description={
                            "See approval status and submitted remarks."
                        }
                        onClick={
                            () =>
                                navigate(
                                    "/reviews"
                                )
                        }
                    />

                </section>

                <section
                    className={
                        "export-strip"
                    }
                >

                    <div>

                        <strong>
                            Ready to share
                            the plan?
                        </strong>

                        <span>
                            Export will use
                            anonymized student
                            information when
                            the backend endpoint
                            is connected.
                        </span>

                    </div>

                    <button
                        className={
                            "secondary-button"
                        }
                        type="button"
                        onClick={
                            () =>
                                window.alert(
                                    "Export endpoint is not implemented yet."
                                )
                        }
                    >
                        Export exam plan
                    </button>

                </section>

            </div>

        </main>
    );
};

export default Main;