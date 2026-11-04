import {useMemo, useState} from "react";
import type {ReactNode} from "react";
import {getCurrentUser, login as loginRequest, logout as logoutRequest} from "../../authApi.ts";
import type {AuthUser} from "../../authTypes.ts";
import {AuthContext} from "./authContext.ts";
import type {AuthContextValue} from "./authContext.ts";

export const AuthProvider = (
    {
        children
    }: {
        children: ReactNode;
    }
) => {

    const [
        user,
        setUser
    ] =
        useState<AuthUser | null>(
            () =>
                getCurrentUser()
        );

    const ready =
        true;

    const value =
        useMemo<AuthContextValue>(
            () => ({
                user,
                ready,

                signIn:
                    async credentials => {

                        const authenticatedUser =
                            await loginRequest(
                                credentials
                            );
                        setUser(
                            authenticatedUser
                        );
                    },

                signOut: () => {
                    logoutRequest();
                    setUser(null);
                }
            }),
            [
                user,
                ready
            ]
        );

    return (
        <AuthContext.Provider
            value={value}
        >
            {children}
        </AuthContext.Provider>
    );
};