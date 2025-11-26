export interface User
{
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    fullName: string;
    phoneNumber?: string;
    role: 'Customer' | 'Agent' | 'Administrator';
    isActive: boolean;
    createdAt: string;
    updatedAt: string;
}

export interface CreateUserRequest
{
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
    role: string;
}

export interface UpdateUserRequest{
    firstName?: string;
    lastName?: string;
    phoneNumber?: string;
    role?: string;
    isActive?: boolean;
}

export interface UpdateProfileRequest {
    firstName: string;
    lastName: string;
    phoneNumber: string;
}