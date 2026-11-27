import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../api/features/auth/AuthContext.tsx";
import { deleteNote, getPlanningElements, saveNote } from "../api/features/access/accessApi.ts";
import type { PlanningElement } from "../api/features/access/accessApi.ts";
import { getErrorMessage } from "../api/errorHandler.ts";
import viaLogo from "./Assets/via-logo-small.png";
import "./Themes/ImportedDataPage.css";

// Teacher notes, for teachers and planners. A note belongs to the exam of a planning element,
// so an element without an exam has nothing to write on yet.
const ExamNotesPage = () => {
    const navigate = useNavigate();
    const { user, signOut } = useAuth();
    const [elements, setElements] = useState<PlanningElement[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [message, setMessage] = useState<string | null>(null);
    const [editing, setEditing] = useState<string | null>(null);
    const [draft, setDraft] = useState("");
    const [busy, setBusy] = useState(false);

    useEffect(() => {
        let cancelled = false;
        getPlanningElements()
            .then(result => { if (!cancelled) setElements(result); })
            .catch(err => { if (!cancelled) setError(getErrorMessage(err)); })
            .finally(() => { if (!cancelled) setLoading(false); });
        return () => { cancelled = true; };
    }, []);

    const startEditing = (element: PlanningElement) => {
        setEditing(element.id);
        setDraft(element.note ?? "");
        setError(null);
        setMessage(null);
    };

    // An empty note means "delete the note".
    const store = async (element: PlanningElement, note: string) => {
        setBusy(true);
        setError(null);
        try {
            if (note) await saveNote(element.id, note);
            else await deleteNote(element.id);
            setElements(list => list.map(e => e.id === element.id ? { ...e, note } : e));
            setEditing(null);
            setMessage(note ? `Note saved for ${element.nickname}.` : `Note deleted for ${element.nickname}.`);
        } catch (err) {
            setError(getErrorMessage(err));
        } finally {
            setBusy(false);
        }
    };

    return (
        <main className="imported-data-page">
            <header className="imported-data-topbar">
                <span className="imported-data-brand">
                    <img src={viaLogo} alt="VIA University College" />
                    <span>Exam Plan</span>
                </span>
                <div className="imported-data-topbar-right">
                    {user && <span className="imported-data-user">{user.name || user.email}</span>}
                    <button type="button" className="imported-secondary-button" onClick={() => navigate("/imported-data")}>
                        Courses
                    </button>
                    <button type="button" className="imported-secondary-button" onClick={() => { signOut(); navigate("/"); }}>
                        Log out
                    </button>
                </div>
            </header>

            <div className="imported-data-title">
                <div>
                    <p className="imported-data-eyebrow">Teacher notes</p>
                    <h1>Exam notes</h1>
                    <p>Notes belong to an exam. A planning element without an exam cannot have a note yet.</p>
                </div>
            </div>

            <div className="imported-data-card">
                {message && <p className="imported-success" role="status">{message}</p>}
                {error && <p className="imported-error" role="alert">{error}</p>}
                {loading && <p className="imported-state">Loading…</p>}
                {!loading && !error && elements.length === 0 && <p className="imported-state">Nothing has been imported yet.</p>}

                {elements.length > 0 && (
                    <div className="imported-table-wrap">
                        <table className="imported-table">
                            <thead>
                                <tr><th>Class</th><th>Course</th><th>Campus</th><th>Note</th><th></th></tr>
                            </thead>
                            <tbody>
                                {elements.map(element => (
                                    <tr key={element.id}>
                                        <td>{element.nickname}</td>
                                        <td>{element.courseCode} {element.courseName}</td>
                                        <td>{element.campus}</td>
                                        <td>
                                            {!element.hasExam ? "No exam yet" : editing === element.id ? (
                                                <textarea
                                                    aria-label={`Note for ${element.nickname}`}
                                                    rows={3}
                                                    maxLength={2000}
                                                    value={draft}
                                                    onChange={event => setDraft(event.target.value)}
                                                />
                                            ) : element.note || "—"}
                                        </td>
                                        <td>
                                            {element.hasExam && editing !== element.id && (
                                                <button type="button" className="imported-edit-button" onClick={() => startEditing(element)}>
                                                    Edit note
                                                </button>
                                            )}
                                            {editing === element.id && (
                                                <>
                                                    <button type="button" className="imported-primary-button" disabled={busy || !draft.trim()}
                                                            onClick={() => void store(element, draft.trim())}>
                                                        Save
                                                    </button>
                                                    <button type="button" className="imported-secondary-button" disabled={busy || !element.note}
                                                            onClick={() => void store(element, "")}>
                                                        Delete note
                                                    </button>
                                                    <button type="button" className="imported-secondary-button" disabled={busy}
                                                            onClick={() => setEditing(null)}>
                                                        Cancel
                                                    </button>
                                                </>
                                            )}
                                        </td>
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

export default ExamNotesPage;
