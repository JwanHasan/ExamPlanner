import Cookies from "js-cookie";

import api, {
    TOKEN_COOKIE
} from "./axiosInstance";

import {
    decodeToken,
    isExpired
} from "./jwtUtils";

import type {
    AuthUser,
    LoginRequest,
    UserRole
} from "./authTypes";

export {
    getErrorMessage
} from "./errorHandler";

const normalizeRole = (
    role?: string
): UserRole => {

    const normalized =
        role?.toLowerCase();

    if (normalized === "teacher") {
        return "Teacher";
    }

    if (normalized === "student") {
        return "Student";
    }

    return "Admin";
};

export const login = async (
    credentials: LoginRequest
): Promise<AuthUser> => {

    const response =
        await api.post<string>(
            "/auth/login",
            credentials
        );

    const token = response.data;

    const payload =
        decodeToken(token);

    if (!payload) {
        throw new Error(
            "The server returned a token that could not be read."
        );
    }

    Cookies.set(
        TOKEN_COOKIE,
        token,
        {
            sameSite: "Lax",
            secure:
                window.location.protocol ===
                "https:",
            expires: 1
        }
    );

    return {
        email:
            payload.email ??
            credentials.email,

        name:
            payload.name ??
            credentials.email.split("@")[0],

        role:
            normalizeRole(
                payload.role
            )
    };
};

export const logout = (): void => {
    Cookies.remove(TOKEN_COOKIE);
};

export const getCurrentUser =
    (): AuthUser | null => {

        const token =
            Cookies.get(TOKEN_COOKIE);

        if (!token) {
            return null;
        }

        const payload =
            decodeToken(token);

        if (
            !payload ||
            isExpired(payload)
        ) {
            Cookies.remove(
                TOKEN_COOKIE
            );

            return null;
        }

        return {
            email:
                payload.email ?? "",

            name:
                payload.name ?? "",

            role:
                normalizeRole(
                    payload.role
                )
        };
    };