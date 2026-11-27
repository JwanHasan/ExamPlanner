import api from "../../axiosInstance.ts";
import type {
    Course,
    UpdateCourseRequest,
} from "./courseTypes.ts";

export const getCourses = async (): Promise<Course[]> => {
    const response = await api.get<Course[]>("/courses");
    return response.data;
};

export const updateCourse = async (
    courseId: number,
    request: UpdateCourseRequest
): Promise<Course> => {
    const response = await api.patch<Course>(
        `/courses/${courseId}`,
        request
    );

    return response.data;
};