import {useMemo, useState} from "react";
import type {ReactNode} from "react";
import {getCurrentUser, login as loginRequest, logout as logoutRequest} from "../../authApi.ts";
import type {AuthUser} from "../../authTypes.ts";
import {AuthContext} from "./authContextDefinition.ts";
import type {AuthContextValue} from "./authContextDefinition.ts";

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
            () => getCurrentUser()
        );
    
    const value =
        useMemo<AuthContextValue>(
            () => ({
                user,
                ready: true,

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
                user
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