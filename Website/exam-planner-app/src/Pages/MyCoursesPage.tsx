import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../api/features/auth/AuthContext.tsx";
import { getMyStudent } from "../api/features/access/accessApi.ts";
import type { MyStudent } from "../api/features/access/accessApi.ts";
import { getErrorMessage } from "../api/errorHandler.ts";
import viaLogo from "./Assets/via-logo-small.png";
import "./Themes/ImportedDataPage.css";

// What a student sees: their own enrollments, nothing else.
const MyCoursesPage = () => {
    const navigate = useNavigate();
    const { user, signOut } = useAuth();
    const [me, setMe] = useState<MyStudent | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        getMyStudent()
            .then(result => { if (!cancelled) setMe(result); })
            .catch(err => { if (!cancelled) setError(getErrorMessage(err)); });
        return () => { cancelled = true; };
    }, []);

    return (
        <main className="imported-data-page">
            <header className="imported-data-topbar">
                <span className="imported-data-brand">
                    <img src={viaLogo} alt="VIA University College" />
                    <span>Exam Plan</span>
                </span>
                <div className="imported-data-topbar-right">
                    {user && <span className="imported-data-user">{user.name || user.email}</span>}
                    <button type="button" className="imported-secondary-button" onClick={() => { signOut(); navigate("/"); }}>
                        Log out
                    </button>
                </div>
            </header>

            <div className="imported-data-title">
                <div>
                    <p className="imported-data-eyebrow">Student {me?.viaId}</p>
                    <h1>My courses</h1>
                    <p>The courses you are enrolled in.</p>
                </div>
            </div>

            <div className="imported-data-card">
                {error && <p className="imported-error" role="alert">{error}</p>}
                {!me && !error && <p className="imported-state">Loading…</p>}
                {me && me.enrollments.length === 0 && <p className="imported-state">You are not enrolled in any imported course yet.</p>}

                {me && me.enrollments.length > 0 && (
                    <div className="imported-table-wrap">
                        <table className="imported-table">
                            <thead>
                                <tr><th>Course</th><th>Class</th><th>Hold</th><th>Campus</th><th>From</th><th>To</th><th>ECTS</th></tr>
                            </thead>
                            <tbody>
                                {me.enrollments.map(e => (
                                    <tr key={e.planningElementId}>
                                        <td>{e.courseCode} {e.courseName}</td>
                                        <td>{e.nickname}</td>
                                        <td>{e.hold}</td>
                                        <td>{e.campus}</td>
                                        <td>{e.startDate}</td>
                                        <td>{e.endDate}</td>
                                        <td>{e.ects}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </main>
    );
};

export default MyCoursesPage;
