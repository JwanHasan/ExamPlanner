import { useState } from "react";
import { examDetails, formatLongDate, formatShortDate, monthBounds } from "../../api/features/calendar/calendarUtils";
import type { PlannedExam } from "../../api/features/calendar/calendarTypes";
import type { Course } from "../../api/features/courses/courseTypes.ts";
import ExamForm from "./ExamForm.tsx";

interface ExamPanelProps {
    year: number;
    month: number;
    monthName: string;
    exams: PlannedExam[];
    selectedDate: string | null;
    courses: Course[];
    coursesLoading: boolean;
    coursesError: string | null;
    onSelectDate: (date: string | null) => void;
    onSave: (exam: PlannedExam) => void;
    onRemove: (id: string) => void;
}

const ExamPanel = (props: ExamPanelProps) => {
    const { year, month, monthName, exams, selectedDate, courses, coursesLoading, coursesError, onSelectDate, onSave, onRemove } = props;
    const [editingId, setEditingId] = useState<string | null>(null);
    const [savedCount, setSavedCount] = useState(0);

    // ---------- Month overview ----------

    if (!selectedDate) {
        return (
            <aside className="cal-panel" aria-label="Exams this month">
                <h2>{monthName} {year}</h2>
                <p className="via-caption">
                    {exams.length === 0 ? "No exams planned yet." : `${exams.length} exam${exams.length === 1 ? "" : "s"} planned.`}{" "}
                    Choose a weekday in the calendar to add or change exams.
                </p>

                {exams.length > 0 && (
                    <ul className="cal-agenda">
                        {exams.map(exam => (
                            <li key={exam.id}>
                                <button type="button" onClick={() => onSelectDate(exam.date)}>
                                    <span className="cal-agenda-date">{formatShortDate(exam.date)}</span>
                                    <span className={`cal-exam${exam.reExam ? " re-exam" : ""}`}>{exam.courseCode}</span>
                                    <span className="cal-agenda-info">{examDetails(exam)}</span>
                                </button>
                            </li>
                        ))}
                    </ul>
                )}
            </aside>
        );
    }

    // ---------- Selected day ----------

    const { first, last } = monthBounds(year, month);
    const dayExams = exams.filter(exam => exam.date === selectedDate);
    const editing = exams.find(exam => exam.id === editingId);

    const remove = (id: string) => {
        if (id === editingId) setEditingId(null);
        onRemove(id);
    };

    return (
        <aside className="cal-panel" aria-label={formatLongDate(selectedDate)}>
            <button type="button" className="cal-link" onClick={() => onSelectDate(null)}>
                « {monthName} overview
            </button>
            <h2>{formatLongDate(selectedDate)}</h2>

            {/* Exams this day */}
            {dayExams.length === 0 ? (
                <p className="via-caption">No exams on this day yet.</p>
            ) : (
                <ul className="cal-day-list">
                    {dayExams.map(exam => (
                        <li key={exam.id} className={`${exam.reExam ? "re-exam" : ""} ${exam.id === editingId ? "editing" : ""}`.trim()}>
                            <div className="cal-exam-text">
                                <strong>
                                    {exam.courseCode}
                                    {exam.reExam && <span className="cal-tag">Re-exam</span>}
                                </strong>
                                {exam.courseName && <span>{exam.courseName}</span>}
                                <span>{examDetails(exam)}</span>
                            </div>
                            <button type="button" className="via-button-secondary via-button-small" onClick={() => setEditingId(exam.id)}>
                                Edit
                            </button>
                            <button
                                type="button"
                                className="cal-remove"
                                aria-label={`Remove ${exam.courseCode}`}
                                onClick={() => remove(exam.id)}
                            >
                                ×
                            </button>
                        </li>
                    ))}
                </ul>
            )}

            {/* Form */}
            <ExamForm
                key={`${editing?.id ?? "new"}-${savedCount}`}
                date={selectedDate}
                firstDate={first}
                lastDate={last}
                monthName={monthName}
                exam={editing}
                courses={courses}
                coursesLoading={coursesLoading}
                coursesError={coursesError}
                onSave={exam => {
                    setEditingId(null);
                    setSavedCount(count => count + 1);
                    onSave(exam);
                }}
                onCancel={() => (editing ? setEditingId(null) : onSelectDate(null))}
            />
        </aside>
    );
};

export default ExamPanel;
