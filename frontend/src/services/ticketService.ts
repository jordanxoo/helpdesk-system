import api from "./api";

import type{
    Ticket,
    CreateTicketRequest,
    AddCommentRequest,
    TicketListResponse,
} from '../types/ticket.types';


export const ticketService = {
    async getAll(page: number = 1, pageSize: number = 10): Promise<TicketListResponse>
    {
        const response = await api.get<TicketListResponse>('/api/tickets',{params: {page,pageSize}});
        return response.data;
    },

    async getMyTickets(page: number = 1, pageSize: number = 10):Promise<TicketListResponse>
    {
        const response = await api.get<TicketListResponse>('/api/tickets/my-tickets',{
            params : {page,pageSize}
        });
        return response.data;
    },

    async getById(id: string) : Promise<Ticket>
    {
        const response = await api.get<Ticket>(`/api/tickets/${id}`);
        return response.data;
    },
    
    async create(data: CreateTicketRequest): Promise<Ticket>{
        const response = await api.post<Ticket>('/api/tickets', data);
        return response.data;
    },
    async addComment(id: string, data: AddCommentRequest): Promise<Ticket> {
        const response = await api.post<Ticket>(`/api/tickets/${id}/comments`,data);
        return response.data;
    },
    async changeStatus(id: string, newStatus: string): Promise<Ticket> {
        const response = await api.patch<Ticket>(`/api/tickets/${id}/status`,{newStatus});
        return response.data;
    }
};