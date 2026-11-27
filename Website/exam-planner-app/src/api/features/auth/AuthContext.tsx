import { createContext, useContext, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import { getCurrentUser, login as loginRequest, logout as logoutRequest } from "../../authApi.ts";
import type { AuthUser, LoginRequest } from "../../authTypes.ts";

interface AuthContextValue {
    user: AuthUser | null;
    ready: boolean;
    signIn: (credentials: LoginRequest) => Promise<void>;
    signOut: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<AuthUser | null>(null);
    const [ready, setReady] = useState(false);

    // Restore the session from the cookie on first load.
    useEffect(() => {
        setUser(getCurrentUser());
        setReady(true);
    }, []);

    const value = useMemo<AuthContextValue>(() => ({
        user,
        ready,
        signIn: async credentials => setUser(await loginRequest(credentials)),
        signOut: () => {
            logoutRequest();
            setUser(null);
        }
    }), [user, ready]);

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextValue => {
    const context = useContext(AuthContext);
    if (!context) throw new Error("useAuth must be used inside AuthProvider");
    return context;
};