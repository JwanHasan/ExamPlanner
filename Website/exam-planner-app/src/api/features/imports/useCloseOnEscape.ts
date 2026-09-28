import { useEffect } from "react";

/**
 * Calls onClose when Escape is pressed, while `active` is true.
 */
export const useCloseOnEscape = (active: boolean, onClose: () => void): void => {
    useEffect(() => {
        if (!active) return;

        const onKey = (event: KeyboardEvent) => {
            if (event.key === "Escape") onClose();
        };

        window.addEventListener("keydown", onKey);
        return () => window.removeEventListener("keydown", onKey);
    }, [active, onClose]);
};