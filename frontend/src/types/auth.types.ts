export interface User
{
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    fullName: string;
    phoneNumber: string;
    role: string;
    createdAt: string;
    updatedAt: string;
    isActive: boolean;
}

export interface LoginRequest
{
    email:string;
    password: string;
}

export interface RegisterRequest
{
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber: string;
    role: string
}

export interface LoginResponse
{
    token: string;
    refreshToken: string;
    expiresAt: string;
    user: User;
}

export interface AuthState
{
    user: User | null;
    token: string | null;   
    isAuthenticated: boolean;
    login : (email: string, password: string) => Promise<void>;
    register: (data: RegisterRequest) => Promise<void>;
    logout: () => void;
}