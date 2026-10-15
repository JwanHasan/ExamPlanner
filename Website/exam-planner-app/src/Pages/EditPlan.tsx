import { useState } from "react";
import type { CSSProperties, FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import viaLogo from "./Assets/via-logo-small.png";
import Dropdown from "../api/features/DropdownExam";
import "./Themes/CalendarPages.css";
import campusBackground from "./Assets/campus-background-transparent.png";

const EXAM_MONTHS = [
    { name: "January", index: 0 },
    { name: "February", index: 1 },
    { name: "June", index: 5 },
    { name: "August", index: 7 }
];

// We might change this when backend gets stuff
const START_YEAR = new Date().getFullYear();

// This will also be replaced with the actual exam data
const PLACEHOLDER_COURSES = ["WEB1", "DNP1", "PRO1", "PRO3"];
const PLACEHOLDER_EXAMS: Record<string, string[]> = {
    "0-5": ["WEB1"],
    "0-9": ["DNP1", "PRO1"],
    "0-13": ["PRO3"]
};

const COURSE_OPTIONS = PLACEHOLDER_COURSES.map(name => ({ value: name, label: name }));

const EditPlan = () => {
    const navigate = useNavigate();

    const [monthPosition, setMonthPosition] = useState(0); // position inside EXAM_MONTHS
    const [year, setYear] = useState(START_YEAR);
    const [selectedDay, setSelectedDay] = useState<number | null>(null);
    const [course, setCourse] = useState("");
    const [duration, setDuration] = useState("");
    const [room, setRoom] = useState("");
    const [isReExam, setIsReExam] = useState(false);

    const { name: monthName, index: month } = EXAM_MONTHS[monthPosition];
    const panelOpen = selectedDay !== null;

    const cols = panelOpen ? 5 : 7;

    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const firstWeekday = (new Date(year, month, 1).getDay() + 6) % 7; // Monday = 0 ... Sunday = 6

    const days = Array.from({ length: daysInMonth }, (_, i) => ({
        day: i + 1,
        isWeekend: (firstWeekday + i) % 7 >= 5
    }));

    const visibleDays = panelOpen ? days.filter(d => !d.isWeekend) : days;
    const leading = panelOpen ? (firstWeekday >= 5 ? 0 : firstWeekday) : firstWeekday;
    const trailing = (cols - ((leading + visibleDays.length) % cols)) % cols;

    const closePanel = () => setSelectedDay(null);

    const openDay = (day: number) => {
        setSelectedDay(day);
        setCourse("");
        setDuration("");
        setRoom("");
        setIsReExam(false);
    };

    const goToPreviousMonth = () => {
        if (monthPosition === 0) {
            setMonthPosition(EXAM_MONTHS.length - 1);
            setYear(year - 1);
        } else {
            setMonthPosition(monthPosition - 1);
        }
        closePanel();
    };

    const goToNextMonth = () => {
        if (monthPosition === EXAM_MONTHS.length - 1) {
            setMonthPosition(0);
            setYear(year + 1);
        } else {
            setMonthPosition(monthPosition + 1);
        }
        closePanel();
    };

    const handleSubmit = (event: FormEvent) => {
        event.preventDefault();
        // Send this to backend when it's made
        closePanel();
    };

    return (
        <main className="calendar-page" style={{ backgroundImage: `url(${campusBackground})` }}>
            <img className="calendar-logo" src={viaLogo} alt="VIA University College" />

            <div className="calendar-workspace">
                <section className="calendar-card" style={{ "--cols": cols } as CSSProperties}>
                    <header className="calendar-header">
                        <button type="button" className="calendar-return" onClick={() => navigate("/Main")}>
                            <span aria-hidden="true">←</span> Return to main page
                        </button>

                        <div className="calendar-nav" aria-live="polite">
                            <div className="calendar-nav-row">
                                <button
                                    type="button"
                                    className="calendar-nav-arrow"
                                    aria-label="Previous month"
                                    onClick={goToPreviousMonth}
                                >
                                    &lt;
                                </button>
                                <h1 className="calendar-nav-month">{monthName}</h1>
                                <button
                                    type="button"
                                    className="calendar-nav-arrow"
                                    aria-label="Next month"
                                    onClick={goToNextMonth}
                                >
                                    &gt;
                                </button>
                            </div>
                            <p className="calendar-nav-year">{year}</p>
                        </div>
                    </header>

                    <div className="calendar-grid">
                        {leading > 0 && (
                            <div className="calendar-filler" style={{ gridColumn: `span ${leading}` }} />
                        )}

                        {visibleDays.map(({ day, isWeekend }) => {
                            const exams = PLACEHOLDER_EXAMS[`${month}-${day}`] ?? [];
                            const content = (
                                <>
                                    <span className="calendar-day-number">{day}</span>
                                    {exams.map(exam => (
                                        <span key={exam} className="calendar-exam">{exam}</span>
                                    ))}
                                </>
                            );

                            if (isWeekend) {
                                return (
                                    <div
                                        key={day}
                                        className="calendar-day calendar-day-weekend"
                                        aria-disabled="true"
                                    >
                                        {content}
                                    </div>
                                );
                            }

                            return (
                                <button
                                    key={day}
                                    type="button"
                                    className={`calendar-day${day === selectedDay ? " calendar-day-selected" : ""}`}
                                    aria-label={`${monthName} ${day}`}
                                    aria-pressed={day === selectedDay}
                                    onClick={() => openDay(day)}
                                >
                                    {content}
                                </button>
                            );
                        })}

                        {trailing > 0 && (
                            <div className="calendar-filler" style={{ gridColumn: `span ${trailing}` }} />
                        )}
                    </div>

                    {/* Sync this to backend when it exists (I'm tired of writing this shit ngl) */}
                    <button type="button" className="calendar-footer">
                        Edit Calendar
                    </button>
                </section>

                {panelOpen && (
                    <form className="exam-panel" onSubmit={handleSubmit}>
                        <div className="exam-panel-header">
                            <button type="button" className="exam-panel-cancel" onClick={closePanel}>
                                Cancel
                            </button>
                            <h2 className="exam-panel-title">{monthName} {selectedDay}</h2>
                        </div>

                        <div className="exam-panel-body">
                            <div className="exam-field">
                                <span className="exam-label" id="exam-course-label">Select the course</span>
                                <Dropdown
                                    ariaLabelledBy="exam-course-label"
                                    value={course}
                                    options={COURSE_OPTIONS}
                                    onChange={setCourse}
                                />
                            </div>

                            <label className="exam-field">
                                <span className="exam-label">Enter the duration of the exam</span>
                                <input
                                    type="number"
                                    min="1"
                                    placeholder="Minutes"
                                    value={duration}
                                    onChange={e => setDuration(e.target.value)}
                                />
                            </label>

                            <label className="exam-field">
                                <span className="exam-label">Enter the room the exam will take place</span>
                                <input
                                    type="text"
                                    value={room}
                                    onChange={e => setRoom(e.target.value)}
                                />
                            </label>

                            <label className="exam-checkbox">
                                <span className="exam-label">Re-exam</span>
                                <input
                                    type="checkbox"
                                    checked={isReExam}
                                    onChange={e => setIsReExam(e.target.checked)}
                                />
                            </label>
                        </div>

                        <button type="submit" className="exam-panel-submit">
                            Edit exam
                        </button>
                    </form>
                )}
            </div>
        </main>
    );
};

export default EditPlan;
