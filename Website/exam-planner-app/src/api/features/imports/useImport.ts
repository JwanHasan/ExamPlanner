import { useState } from "react";
import { uploadImport } from "./importApi";
import { getErrorMessage } from "../../errorHandler";
import type { ImportResult } from "./importTypes";

export interface ImportState {
    open: boolean;
    busy: boolean;
    progress: number;
    fileName: string;
    result: ImportResult | null;
    error: string | null;
}

const initialState: ImportState = {
    open: false,
    busy: false,
    progress: 0,
    fileName: "",
    result: null,
    error: null
};

export const useImport = () => {
    const [state, setState] = useState<ImportState>(initialState);

    const start = async (file: File) => {
        setState({ ...initialState, open: true, busy: true, fileName: file.name });

        try {
            const result = await uploadImport(file, progress =>
                setState(s => ({ ...s, progress }))
            );
            setState(s => ({ ...s, busy: false, result }));
        } catch (err) {
            setState(s => ({ ...s, busy: false, error: getErrorMessage(err) }));
        }
    };

    const close = () => setState(s => (s.busy ? s : initialState));

    return { state, start, close };
};