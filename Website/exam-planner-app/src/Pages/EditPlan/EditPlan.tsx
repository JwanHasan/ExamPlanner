import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCourses } from "../../api/features/courses/useCourses.ts";
import { useExamPlan } from "../../api/features/calendar/useExamPlan";
import { EXAM_MONTHS, sortExams, stepExamMonth, toIsoDate, upcomingExamMonth } from "../../api/features/calendar/calendarUtils";
import type { PlannedExam } from "../../api/features/calendar/calendarTypes";
import TopBar from "../Shared/TopBar.tsx";
import CalendarGrid from "./CalendarGrid.tsx";
import ExamPanel from "./ExamPanel.tsx";
import "../Themes/MitVia.css";
import "../Themes/EditPlan.css";

const EditPlan = () => {
    const navigate = useNavigate();
    const courses = useCourses();
    const plan = useExamPlan();

    const [start] = useState(() => upcomingExamMonth(new Date()));
    const [year, setYear] = useState(start.year);
    const [position, setPosition] = useState(start.position);
    const [selectedDate, setSelectedDate] = useState<string | null>(null);

    const { name: monthName, index: month } = EXAM_MONTHS[position];
    const monthPrefix = toIsoDate(year, month, 1).slice(0, 7);
    const monthExams = sortExams(plan.exams.filter(exam => exam.date.startsWith(monthPrefix)));

    const showMonth = (nextYear: number, nextPosition: number) => {
        setYear(nextYear);
        setPosition(nextPosition);
        setSelectedDate(null);
    };

    const step = (by: number) => {
        const next = stepExamMonth(year, position, by);
        showMonth(next.year, next.position);
    };

    const saveExam = (exam: PlannedExam) => {
        plan.saveExam(exam);
        setSelectedDate(exam.date);
    };

    return (
        <div className="via-page">
            {/* Top bar */}
            <TopBar />

            {/* Title bar */}
            <div className="via-titlebar">
                <h1>Edit exam plan</h1>
                <div className="cal-month-nav">
                    <button type="button" aria-label="Previous exam month" onClick={() => step(-1)}>‹</button>
                    <span aria-live="polite">{monthName} {year}</span>
                    <button type="button" aria-label="Next exam month" onClick={() => step(1)}>›</button>
                </div>
            </div>

            <main className="via-content">
                <div className="cal-layout">
                    {/* Calendar */}
                    <section className="cal-card" aria-label={`Calendar for ${monthName} ${year}`}>
                        <div className="cal-toolbar">
                            <div className="via-filter" aria-label={`Exam months ${year}`}>
                                {EXAM_MONTHS.map((item, index) => (
                                    <button
                                        key={item.name}
                                        type="button"
                                        className={`via-chip ${index === position ? "active" : ""}`}
                                        aria-pressed={index === position}
                                        onClick={() => showMonth(year, index)}
                                    >
                                        {item.name}
                                    </button>
                                ))}
                            </div>
                            <ul className="cal-legend">
                                <li>Exam</li>
                                <li className="re-exam">Re-exam</li>
                                <li className="weekend">Weekend</li>
                            </ul>
                        </div>

                        <CalendarGrid
                            year={year}
                            month={month}
                            exams={monthExams}
                            selectedDate={selectedDate}
                            onSelect={setSelectedDate}
                        />
                    </section>

                    {/* Side panel */}
                    <ExamPanel
                        key={selectedDate ?? "month"}
                        year={year}
                        month={month}
                        monthName={monthName}
                        exams={monthExams}
                        selectedDate={selectedDate}
                        courses={courses.courses}
                        coursesLoading={courses.loading}
                        coursesError={courses.error}
                        onSelectDate={setSelectedDate}
                        onSave={saveExam}
                        onRemove={plan.removeExam}
                    />
                </div>

                {/* Buttons */}
                <footer className="via-actions">
                    <button type="button" className="via-button-secondary" onClick={() => navigate("/Main")}>
                        « Main page
                    </button>
                </footer>
            </main>
        </div>
    );
};

export default EditPlan;
