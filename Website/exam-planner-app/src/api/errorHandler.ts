import axios from "axios";

export const getErrorMessage = (error: unknown): string => {
    if (axios.isAxiosError(error)) {
        if (!error.response) {
            return "Cannot reach the server. Check that the API is running.";
        }

        const status = error.response.status;
        const data = error.response.data;

        if (status === 401) return "Email or password is incorrect.";
        if (status === 403) return "This account does not have access to Exam Plan.";
        if (status >= 500) return "The server ran into a problem. Try again in a moment.";

        if (typeof data === "string" && data.trim()) return data;
        if (data && typeof data === "object" && "message" in data) {
            return String((data as { message: unknown }).message);
        }
        return "Something went wrong. Try again.";
    }

    if (error instanceof Error) return error.message;
    return "Something went wrong. Try again.";
};