import {
    Navigate,
    useLocation
} from "react-router-dom";

import type {
    ReactNode
} from "react";

import {
    useAuth
} from "./AuthContext";

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

    if (!user) {
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