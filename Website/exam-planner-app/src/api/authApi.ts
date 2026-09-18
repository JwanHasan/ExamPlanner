import api, { TOKEN_COOKIE } from "./axiosInstance.ts";
import Cookies from "js-cookie";
import { decodeToken, isExpired } from "./jwtUtils";
import type { AuthUser, LoginRequest } from "./authTypes";

export { getErrorMessage } from "./errorHandler";

/**
 * POST /api/auth/login
 * The api answers with a JWT as a plain string.
 */
export const login = async (credentials: LoginRequest): Promise<AuthUser> => {
    const response = await api.post<string>("/auth/login", credentials);
    const token = response.data;

    const payload = decodeToken(token);
    if (!payload) {
        throw new Error("The server returned a token that could not be read.");
    }

    Cookies.set(TOKEN_COOKIE, token, {
        sameSite: "Lax",
        secure: window.location.protocol === "https:",
        expires: 1
    });

    return {
        email: payload.email ?? credentials.email,
        name: payload.name ?? "",
        role: payload.role ?? "planner"
    };
};

export const logout = (): void => {
    Cookies.remove(TOKEN_COOKIE);
};

/**
 * Reads the current user from the stored token, or null if there is
 * no token or it has expired. Used to restore the session on reload.
 */
export const getCurrentUser = (): AuthUser | null => {
    const token = Cookies.get(TOKEN_COOKIE);
    if (!token) return null;

    const payload = decodeToken(token);
    if (!payload || isExpired(payload)) {
        Cookies.remove(TOKEN_COOKIE);
        return null;
    }

    return {
        email: payload.email ?? "",
        name: payload.name ?? "",
        email: "",
    };
};