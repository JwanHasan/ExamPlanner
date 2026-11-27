export interface Course {
    id: number;
    name: string;
    semester: string;
    sueCode: string;
    ects: number;
    notes: string | null;
    classCount: number;
}

export interface UpdateCourseRequest {
    name: string;
    sueCode: string;
    notes: string | null;
    classCount: number;
}