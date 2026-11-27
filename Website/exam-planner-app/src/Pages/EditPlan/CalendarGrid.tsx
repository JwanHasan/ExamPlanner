import { Fragment } from "react";
import { WEEKDAYS, buildMonth, formatLongDate } from "../../api/features/calendar/calendarUtils";
import type { CalendarDay, PlannedExam } from "../../api/features/calendar/calendarTypes";

const MAX_CHIPS = 3;

interface CalendarGridProps {
    year: number;
    month: number;
    exams: PlannedExam[];
    selectedDate: string | null;
    onSelect: (date: string) => void;
}

const CalendarGrid = ({ year, month, exams, selectedDate, onSelect }: CalendarGridProps) => {
    const examsOn = (date: string) => exams.filter(exam => exam.date === date);

    const dayCell = (day: CalendarDay) => {
        const dayExams = examsOn(day.date);
        const content = (
            <>
                <span className="cal-day-number">{day.day}</span>
                {dayExams.slice(0, MAX_CHIPS).map(exam => (
                    <span key={exam.id} className={`cal-exam${exam.reExam ? " re-exam" : ""}`}>
                        {exam.courseCode}
                    </span>
                ))}
                {dayExams.length > MAX_CHIPS && <span className="cal-more">+{dayExams.length - MAX_CHIPS} more</span>}
            </>
        );

        if (day.isWeekend) {
            return <div key={day.date} className="cal-day cal-day-weekend">{content}</div>;
        }

        const selected = day.date === selectedDate;
        return (
            <button
                key={day.date}
                type="button"
                className={`cal-day${selected ? " selected" : ""}`}
                aria-pressed={selected}
                aria-label={`${formatLongDate(day.date)}, ${dayExams.length} exam${dayExams.length === 1 ? "" : "s"}`}
                onClick={() => onSelect(day.date)}
            >
                {content}
            </button>
        );
    };

    return (
        <div className="cal-grid">
            {/* Weekday names */}
            <div className="cal-head cal-week-head">Week</div>
            {WEEKDAYS.map((name, index) => (
                <div key={name} className={`cal-head${index >= 5 ? " weekend" : ""}`}>{name}</div>
            ))}

            {/* Weeks */}
            {buildMonth(year, month).map(week => (
                <Fragment key={week.number}>
                    <div className="cal-week">{week.number}</div>
                    {week.days.map((day, index) =>
                        day ? dayCell(day) : <div key={`empty-${week.number}-${index}`} className="cal-day cal-day-empty" />
                    )}
                </Fragment>
            ))}
        </div>
    );
};

export default CalendarGrid;
