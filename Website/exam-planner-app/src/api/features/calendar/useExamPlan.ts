import { useState } from "react";
import type { PlannedExam } from "./calendarTypes";
import { EXAM_MONTHS, nthWeekday, upcomingExamMonth } from "./calendarUtils";

const placeholderExams = (): PlannedExam[] => {
    const { year, position } = upcomingExamMonth(new Date());
    const month = EXAM_MONTHS[position].index;
    const exam = (id: string, weekday: number, courseCode: string): PlannedExam => ({
        id,
        date: nthWeekday(year, month, weekday),
        courseId: null,
        courseCode,
        courseName: "",
        duration: null,
        room: "",
        reExam: false
    });

    return [exam("placeholder-1", 3, "WEB1"), exam("placeholder-2", 7, "DNP1"), exam("placeholder-3", 7, "PRO1"), exam("placeholder-4", 10, "PRO3")];
};

export const useExamPlan = () => {
    const [exams, setExams] = useState<PlannedExam[]>(placeholderExams);

    const saveExam = (exam: PlannedExam) =>
        setExams(list => (list.some(item => item.id === exam.id)
            ? list.map(item => (item.id === exam.id ? exam : item))
            : [...list, exam]));

    const removeExam = (id: string) => setExams(list => list.filter(item => item.id !== id));

    return { exams, saveExam, removeExam };
};
