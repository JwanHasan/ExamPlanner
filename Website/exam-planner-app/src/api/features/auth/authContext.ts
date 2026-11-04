import {createContext} from "react";
import type {AuthUser, LoginRequest} from "../../authTypes.ts";

export interface AuthContextValue {
    user: AuthUser | null;

    ready: boolean;

    signIn: (
        credentials: LoginRequest
    ) => Promise<void>;

    signOut: () => void;
}

export const AuthContext =
    createContext<
        AuthContextValue | undefined
    >(undefined);