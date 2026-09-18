export interface LoginRequest {
    email: string;
    password: string;
}

export interface AuthUser {
    email: string;
    name: string;
    role: string;
}

export interface JwtPayload {
    sub?: string;
    email?: string;
    name?: string;
    role?: string;
    exp?: number;
    [key: string]: unknown;
}