import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { isWeekend, newExamId } from "../../api/features/calendar/calendarUtils";
import type { PlannedExam } from "../../api/features/calendar/calendarTypes";
import type { Course } from "../../api/features/courses/courseTypes.ts";

const CURRENT_COURSE = "current";

interface ExamFormProps {
    date: string;
    firstDate: string;
    lastDate: string;
    monthName: string;
    exam?: PlannedExam;
    courses: Course[];
    coursesLoading: boolean;
    coursesError: string | null;
    onSave: (exam: PlannedExam) => void;
    onCancel: () => void;
}

const ExamForm = (props: ExamFormProps) => {
    const { date, firstDate, lastDate, monthName, exam, courses, coursesLoading, coursesError, onSave, onCancel } = props;
    const navigate = useNavigate();

    const keepsCurrentCourse = exam !== undefined && exam.courseId === null;
    const [courseKey, setCourseKey] = useState(exam ? (keepsCurrentCourse ? CURRENT_COURSE : String(exam.courseId)) : "");
    const [examDate, setExamDate] = useState(exam?.date ?? date);
    const [duration, setDuration] = useState(exam?.duration ? String(exam.duration) : "");
    const [room, setRoom] = useState(exam?.room ?? "");
    const [reExam, setReExam] = useState(exam?.reExam ?? false);
    const [problem, setProblem] = useState<string | null>(null);

    const sortedCourses = [...courses].sort((a, b) => a.sueCode.localeCompare(b.sueCode) || a.name.localeCompare(b.name));

    const submit = (event: FormEvent) => {
        event.preventDefault();

        const course = courses.find(item => String(item.id) === courseKey);
        if (!course && courseKey !== CURRENT_COURSE) return setProblem("Choose a course.");
        if (!examDate || examDate < firstDate || examDate > lastDate) return setProblem(`Choose a date in ${monthName}.`);
        if (isWeekend(examDate)) return setProblem("Exams can only be planned on weekdays.");

        const minutes = duration.trim() === "" ? null : Number(duration);
        if (minutes !== null && (!Number.isInteger(minutes) || minutes <= 0)) {
            return setProblem("The duration must be a whole number of minutes.");
        }

        onSave({
            id: exam?.id ?? newExamId(),
            date: examDate,
            courseId: course ? course.id : null,
            courseCode: course ? course.sueCode : exam?.courseCode ?? "",
            courseName: course ? course.name : exam?.courseName ?? "",
            duration: minutes,
            room: room.trim(),
            reExam
        });
    };

    return (
        <form className="cal-form" onSubmit={submit} noValidate>
            <h3>{exam ? `Edit ${exam.courseCode}` : "Add exam"}</h3>

            {/* Course */}
            <label className="cal-field">
                <span>Course</span>
                <select value={courseKey} onChange={event => setCourseKey(event.target.value)} disabled={coursesLoading}>
                    <option value="">{coursesLoading ? "Loading courses…" : "Choose a course"}</option>
                    {keepsCurrentCourse && <option value={CURRENT_COURSE}>{exam.courseCode} (current)</option>}
                    {sortedCourses.map(course => (
                        <option key={course.id} value={String(course.id)}>
                            {course.sueCode} – {course.name}
                        </option>
                    ))}
                </select>
            </label>
            {coursesError && <p className="via-field-error" role="alert">The courses could not be loaded: {coursesError}</p>}
            {!coursesLoading && !coursesError && courses.length === 0 && (
                <p className="cal-note">
                    No courses have been imported yet.{" "}
                    <button type="button" className="cal-link" onClick={() => navigate("/import")}>Import data</button>
                </p>
            )}

            {/* Date and duration */}
            <div className="cal-field-row">
                <label className="cal-field">
                    <span>Date</span>
                    <input type="date" value={examDate} min={firstDate} max={lastDate} onChange={event => setExamDate(event.target.value)} />
                </label>
                <label className="cal-field">
                    <span>Duration (minutes)</span>
                    <input type="number" inputMode="numeric" min="1" step="1" value={duration} onChange={event => setDuration(event.target.value)} />
                </label>
            </div>

            {/* Room */}
            <label className="cal-field">
                <span>Room</span>
                <input type="text" placeholder="For example C05.16" value={room} onChange={event => setRoom(event.target.value)} />
            </label>

            <label className="via-check">
                <input type="checkbox" checked={reExam} onChange={event => setReExam(event.target.checked)} />
                Re-exam
            </label>

            {problem && <p className="via-field-error" role="alert">{problem}</p>}

            {/* Buttons */}
            <div className="cal-form-actions">
                <button type="button" className="via-button-secondary" onClick={onCancel}>
                    Cancel
                </button>
                <button type="submit" className="via-button">
                    {exam ? "Save changes" : "Add exam"}
                </button>
            </div>
        </form>
    );
};

export default ExamForm;
