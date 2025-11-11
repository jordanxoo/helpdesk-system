import api from './api';

import type { LoginRequest, RegisterRequest, LoginResponse } from '../types/auth.types';

export const authService = {
    async login(credentials: LoginRequest): Promise<LoginResponse> {
        const response = await api.post<LoginResponse>('/api/auth/login', credentials);
        return response.data;
    },

    async register(data: RegisterRequest) : Promise<LoginResponse>{
        const response = await api.post<LoginResponse>('/api/auth/register', data);
        return response.data;
    },

    async logout(): Promise<void> {
        await api.post('/api/auth/logout');
    },
};