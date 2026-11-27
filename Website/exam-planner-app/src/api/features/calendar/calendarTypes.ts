export interface PlannedExam {
    id: string;
    date: string;
    courseId: number | null;
    courseCode: string;
    courseName: string;
    duration: number | null;
    room: string;
    reExam: boolean;
}

export interface ExamMonth {
    name: string;
    index: number;
}

export interface CalendarDay {
    date: string;
    day: number;
    isWeekend: boolean;
}

export interface CalendarWeek {
    number: number;
    days: (CalendarDay | null)[];
}
