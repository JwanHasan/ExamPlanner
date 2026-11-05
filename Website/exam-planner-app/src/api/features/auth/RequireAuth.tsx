import { Navigate, useLocation } from "react-router-dom";
import type { ReactNode } from "react";
import { getCurrentUser } from "../../authApi.ts";
import { useAuth } from "./useAuth.ts";

export const RequireAuth = (
    {
        children
    }: {
        children: ReactNode;
    }
) => {
    const {
        user,
        ready
    } =
        useAuth();

    const location =
        useLocation();

    if (!ready) {
        return null;
    }

    const authenticatedUser =
        user ??
        getCurrentUser();
    
    if (!authenticatedUser) {
        return (
            <Navigate
                to="/"
                replace
                state={{
                    from:
                    location.pathname
                }}
            />
        );
    }
    
    return (
        <>
            {children}
        </>
    );
};