import {useContext} from "react";
import {AuthContext} from "./authContextDefinition.ts";
import type {AuthContextValue} from "./authContextDefinition.ts";

export const useAuth =
    (): AuthContextValue => {
        const context =
            useContext(
                AuthContext
            );
        
        if (!context) {
            throw new Error(
                "useAuth must be used inside AuthProvider"
            );
        }
        
        return context;
    };