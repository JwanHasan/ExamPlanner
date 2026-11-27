import api from "../../axiosInstance.ts";

export interface PlanningElement {
    id: string;
    name: string;
    nickname: string;
    courseId: number;
    courseCode: string;
    courseName: string;
    campus: string;
    ects: number;
    startDate: string;
    endDate: string;
    hasExam: boolean;
    note: string | null;
}

export interface MyEnrollment {
    planningElementId: string;
    name: string;
    nickname: string;
    courseCode: string;
    courseName: string;
    campus: string;
    hold: string;
    ects: number;
    startDate: string;
    endDate: string;
}

export interface MyStudent {
    id: number;
    viaId: string;
    name: string;
    attemptNo: number;
    enrollments: MyEnrollment[];
}

export const getPlanningElements = async (): Promise<PlanningElement[]> =>
    (await api.get<PlanningElement[]>("/planning-elements")).data;

export const saveNote = async (elementId: string, note: string): Promise<void> => {
    await api.put(`/planning-elements/${elementId}/note`, { note });
};

export const deleteNote = async (elementId: string): Promise<void> => {
    await api.delete(`/planning-elements/${elementId}/note`);
};

export const getMyStudent = async (): Promise<MyStudent> =>
    (await api.get<MyStudent>("/students/me")).data;
