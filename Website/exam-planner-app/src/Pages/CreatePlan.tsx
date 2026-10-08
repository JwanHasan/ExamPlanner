import viaLogo from "./Assets/via-logo-small.png";
import campusBackground from "./Assets/campus-background-transparent.png";
import "./Themes/CalendarPages.css";
import { useState } from "react";
import type { CSSProperties, FormEvent } from "react";
import DropdownExam from "../api/features/DropdownExam.tsx";
import DropdownMonth from "../api/features/DropdownMonth.tsx";
import { useNavigate } from "react-router-dom";

const MONTHS = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];

const YEAR = new Date().getFullYear();

//Since backend don't exist this is our temp data to show how exams are gonna look in final calendar
const PLACEHOLDER_COURSES = ["WEB1", "DNP1", "PRO1", "PRO3"];
const PLACEHOLDER_EXAMS: Record<string, string[]> = {
    "0-5": ["WEB1"],
    "0-9": ["DNP1", "PRO1"],
    "0-12": ["PRO3"]
};

const MONTH_OPTIONS = MONTHS.map((name, index) => ({ value: String(index), label: name }));
const COURSE_OPTIONS = PLACEHOLDER_COURSES.map(name => ({ value: name, label: name }));

const CreatePlan = () => {

    const navigate = useNavigate();

    const [month, setMonth] = useState(0);
    const [selectedDay, setSelectedDay] = useState<number | null>(null);
    const [course, setCourse] = useState("");
    const [duration, setDuration] = useState("");
    const [room, setRoom] = useState("");

    const panelOpen = selectedDay !== null;

    const cols = panelOpen ? 5 : 7;

    const daysInMonth = new Date(YEAR, month + 1, 0).getDate();
    const firstWeekday = (new Date(YEAR, month, 1).getDay() + 6) % 7; // Monday = 0 ... Sunday = 6

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
    };

    const handleMonthChange = (value: string) => {
        setMonth(Number(value));
        closePanel();
    };

    const handleSubmit = (event: FormEvent) => {
        event.preventDefault();
        //This is gonna be sent to backend when it exists
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

                        <DropdownMonth
                            className="calendar-month"
                            ariaLabel="Month"
                            value={String(month)}
                            options={MONTH_OPTIONS}
                            onChange={handleMonthChange}
                        />
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
                                    aria-label={`${MONTHS[month]} ${day}`}
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

                    {/*Link to backend after it's created*/}
                    <button type="button" className="calendar-footer">
                        Create Calendar
                    </button>
                </section>

                {panelOpen && (
                    <form className="exam-panel" onSubmit={handleSubmit}>
                        <div className="exam-panel-header">
                            <button type="button" className="exam-panel-cancel" onClick={closePanel}>
                                Cancel
                            </button>
                            <h2 className="exam-panel-title">{MONTHS[month]} {selectedDay}</h2>
                        </div>

                        <div className="exam-panel-body">
                            <label className="exam-field">
                                <span className="exam-label">Select the course</span>
                                <DropdownExam
                                    ariaLabelledBy="exam-course-label"
                                    value={course}
                                    options={COURSE_OPTIONS}
                                    onChange={setCourse}
                                />
                            </label>

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
                        </div>

                        <button type="submit" className="exam-panel-submit">
                            Create exam
                        </button>
                    </form>
                )}
            </div>
        </main>
    );
};

export default CreatePlan;
