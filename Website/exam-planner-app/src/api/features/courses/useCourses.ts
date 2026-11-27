import { useEffect, useState } from "react";
import { getCourses } from "./courseApi.ts";
import type { Course } from "./courseTypes.ts";
import { getErrorMessage } from "../../errorHandler.ts";

export const useCourses = () => {
    const [courses, setCourses] = useState<Course[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        getCourses()
            .then(result => {
                if (!cancelled) setCourses(result);
            })
            .catch(err => {
                if (!cancelled) setError(getErrorMessage(err));
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });
        return () => {
            cancelled = true;
        };
    }, []);

    return { courses, loading, error };
};
