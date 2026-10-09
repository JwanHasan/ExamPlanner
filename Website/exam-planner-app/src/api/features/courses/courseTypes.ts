export interface Course {
    id: number;
    name: string;
    semester: string;
    sueCode: string;
    ects: number;
    notes: string | null;
    classCount: number;
}

export interface UpdateCourseNotesRequest {
    notes: string | null;
}