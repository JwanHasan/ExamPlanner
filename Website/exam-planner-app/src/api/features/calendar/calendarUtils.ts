import type { CalendarDay, CalendarWeek, ExamMonth, PlannedExam } from "./calendarTypes";

export const EXAM_MONTHS: ExamMonth[] = [
    { name: "January", index: 0 },
    { name: "February", index: 1 },
    { name: "June", index: 5 },
    { name: "August", index: 7 }
];

export const WEEKDAYS = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

const DAY_NAMES = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
const MONTH_NAMES = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];

const pad = (value: number) => String(value).padStart(2, "0");

// ---------- Dates ----------

export const toIsoDate = (year: number, month: number, day: number): string =>
    `${year}-${pad(month + 1)}-${pad(day)}`;

const fromIsoDate = (date: string): Date => {
    const [year, month, day] = date.split("-").map(Number);
    return new Date(year, month - 1, day);
};

const weekdayOf = (date: Date): number => (date.getDay() + 6) % 7;

export const isWeekend = (date: string): boolean => weekdayOf(fromIsoDate(date)) >= 5;

const isoWeek = (year: number, month: number, day: number): number => {
    const date = new Date(Date.UTC(year, month, day));
    date.setUTCDate(date.getUTCDate() + 4 - (date.getUTCDay() || 7));
    const yearStart = Date.UTC(date.getUTCFullYear(), 0, 1);
    return Math.ceil(((date.getTime() - yearStart) / 86400000 + 1) / 7);
};

export const monthBounds = (year: number, month: number) => ({
    first: toIsoDate(year, month, 1),
    last: toIsoDate(year, month, new Date(year, month + 1, 0).getDate())
});

export const formatLongDate = (date: string): string => {
    const value = fromIsoDate(date);
    return `${DAY_NAMES[weekdayOf(value)]} ${value.getDate()} ${MONTH_NAMES[value.getMonth()]} ${value.getFullYear()}`;
};

export const formatShortDate = (date: string): string => {
    const value = fromIsoDate(date);
    return `${WEEKDAYS[weekdayOf(value)]} ${value.getDate()} ${MONTH_NAMES[value.getMonth()].slice(0, 3)}`;
};

// ---------- Month grid ----------

export const buildMonth = (year: number, month: number): CalendarWeek[] => {
    const offset = weekdayOf(new Date(year, month, 1));
    const daysInMonth = new Date(year, month + 1, 0).getDate();

    const cells: (CalendarDay | null)[] = Array.from({ length: offset }, () => null);
    for (let day = 1; day <= daysInMonth; day++) {
        cells.push({ date: toIsoDate(year, month, day), day, isWeekend: (offset + day - 1) % 7 >= 5 });
    }
    while (cells.length % 7 !== 0) cells.push(null);

    const weeks: CalendarWeek[] = [];
    for (let start = 0; start < cells.length; start += 7) {
        const days = cells.slice(start, start + 7);
        const firstDay = days.find(day => day !== null)!;
        weeks.push({ number: isoWeek(year, month, firstDay.day), days });
    }
    return weeks;
};

// ---------- Exam months ----------

export const upcomingExamMonth = (today: Date) => {
    const position = EXAM_MONTHS.findIndex(month => month.index >= today.getMonth());
    return position >= 0
        ? { year: today.getFullYear(), position }
        : { year: today.getFullYear() + 1, position: 0 };
};

export const stepExamMonth = (year: number, position: number, step: number) => {
    const total = year * EXAM_MONTHS.length + position + step;
    return { year: Math.floor(total / EXAM_MONTHS.length), position: total % EXAM_MONTHS.length };
};

export const nthWeekday = (year: number, month: number, n: number): string => {
    let count = 0;
    for (let day = 1; ; day++) {
        if (weekdayOf(new Date(year, month, day)) < 5 && ++count === n) return toIsoDate(year, month, day);
    }
};

// ---------- Exams ----------

let nextId = 1;

export const newExamId = (): string => `exam-${Date.now()}-${nextId++}`;

export const examDetails = (exam: PlannedExam): string =>
    [exam.duration ? `${exam.duration} min` : "", exam.room].filter(Boolean).join(" · ") || "No duration or room yet";

export const sortExams = (exams: PlannedExam[]): PlannedExam[] =>
    [...exams].sort((a, b) => a.date.localeCompare(b.date) || a.courseCode.localeCompare(b.courseCode));
