import api from "../../axiosInstance.ts";
import type { ImportResult } from "./importTypes";


export const uploadImport = async (
    file: File,
    onProgress?: (percent: number) => void
): Promise<ImportResult> => {
    const form = new FormData();
    form.append("file", file);

    // POST /imports (API_CONTRACT.md 6.1)
    const response = await api.post<ImportResult>("/imports", form, {
        headers: { "Content-Type": "multipart/form-data" },
        onUploadProgress: event => {
            if (onProgress && event.total) {
                onProgress(Math.round((event.loaded / event.total) * 100));
            }
        }
    });

    return response.data;
};