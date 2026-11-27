export interface LoginRequest {
    email: string;
    password: string;
}

// Body of POST /auth/login (API_CONTRACT.md 5.1)
export interface LoginResponse {
    accessToken: string;
    expiresAt: string;
    user: {
        id: number;
        email: string;
        role: string;
    };
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