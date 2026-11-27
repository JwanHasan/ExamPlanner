// The roles the API puts in the login token, and the page each of them starts on.
export const ADMIN = "Admin";
export const TEACHER = "Teacher";
export const STUDENT = "Student";

export const homeFor = (role: string | undefined): string => {
    if (role === ADMIN) return "/Main";
    if (role === TEACHER) return "/imported-data";
    if (role === STUDENT) return "/my-courses";
    return "/";
};
