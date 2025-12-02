import api from "./api";
import type {User, CreateUserRequest, UpdateUserRequest, UpdateProfileRequest} from '../types/user.types';


export const userService = {
    async getAllUsers(): Promise<User[]> {
    const response = await api.get<User[]>('/api/users');
    return response.data;
    },

    async getUserById(id: string): Promise<User>{
        const response = await api.get<User>(`/api/users/${id}`);
        return response.data;
    },

    async createUser(data: CreateUserRequest): Promise<User>{
        const response = await api.post<User>(`/api/users`,data);
        return response.data;
    },
    async updateUser(id:string, data: UpdateUserRequest): Promise<User>{
        const response = await api.put<User>(`/api/users/${id}`,data);
        return response.data;
    },
    async deleteUser(id:string): Promise<void>{
         await api.delete(`/api/users/${id}`);
    },
    async getProfile(): Promise<User>{
        const response = await api.get<User>(`/api/users/me`);
        return response.data;
    },

    async updateProfile(userId: string, data: UpdateProfileRequest): Promise<User>
    {
        const response = await api.put<User>(`/api/users/${userId}`,data);
        return response.data;
    }
}