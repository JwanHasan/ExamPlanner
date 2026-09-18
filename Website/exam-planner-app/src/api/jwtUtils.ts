import type { JwtPayload } from "./authTypes";

/**
 * Decodes the payload of a JWT without verifying the signature.
 * Verification happens on the server. This is only used to read
 * the claims so the UI knows who is logged in.
 */
export const decodeToken = (token: string): JwtPayload | null => {
    try {
        const payload = token.split(".")[1];
        if (!payload) return null;

        const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
        const json = decodeURIComponent(
            atob(normalized)
                .split("")
                .map(c => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
                .join("")
        );

        return JSON.parse(json) as JwtPayload;
    } catch {
        return null;
    }
};

export const isExpired = (payload: JwtPayload): boolean => {
    if (!payload.exp) return false;
    return payload.exp * 1000 <= Date.now();
};