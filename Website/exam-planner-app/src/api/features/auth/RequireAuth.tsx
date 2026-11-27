import { Navigate, useLocation } from "react-router-dom";
import type { ReactNode } from "react";
import { useAuth } from "./AuthContext.tsx";
import { homeFor } from "./roles.ts";

// Pages for logged-in users, optionally only for some roles. This only keeps the screens in order:
// the API checks the role on every request, whatever the website shows.
export const RequireAuth = ({ children, roles }: { children: ReactNode; roles?: string[] }) => {
    const { user, ready } = useAuth();
    const location = useLocation();

    if (!ready) return null;
    if (!user) return <Navigate to="/" replace state={{ from: location.pathname }} />;
    if (roles && !roles.includes(user.role)) return <Navigate to={homeFor(user.role)} replace />;

    return <>{children}</>;
};
