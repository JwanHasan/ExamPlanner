import api from "../../axiosInstance.ts";
import type {
    Course,
    UpdateCourseNotesRequest
} from "./courseTypes.ts";

export const getCourses = async (): Promise<Course[]> => {
    const response = await api.get<Course[]>("/courses");
    return response.data;
};

export const updateCourseNotes = async (
    courseId: number,
    request: UpdateCourseNotesRequest
): Promise<Course> => {
    const response = await api.patch<Course>(
        `/courses/${courseId}`,
        request
    );

    return response.data;
};