import api from './api';
import type { Ticket, CreateTicketRequest, TicketComment, TicketDetails, TicketListResponse } from '../types/ticket.types';

// Interface for paginated response from backend
interface TicketApiResponse {
    tickets: Ticket[];
    totalCount: number;
    page: number;
    pageSize: number;
}

export const ticketService = {
    async getMyTickets(): Promise<TicketApiResponse> {
        const response = await api.get<TicketApiResponse>('/api/tickets/my-tickets');
        return response.data;
    },

    async getAllTickets(): Promise<TicketApiResponse> {
        const response = await api.get<TicketApiResponse>('/api/tickets');
        return response.data;
    },

    async getTicketById(id: string): Promise<TicketDetails> {
        const response = await api.get<TicketDetails>(`/api/tickets/${id}`);
        return response.data;
    },

    async createTicket(data: CreateTicketRequest): Promise<Ticket> {
        const response = await api.post<Ticket>('/api/tickets', data);
        return response.data;
    },

    async updateTicketStatus(id: string, status: string): Promise<Ticket> {
        const response = await api.patch<Ticket>(`/api/tickets/${id}/status`, { newStatus: status });
        return response.data;
    },

    async updateTicketPriority(id: string, priority: string): Promise<Ticket> {
        const response = await api.patch<Ticket>(`/api/tickets/${id}/priority`, { newPriority: priority });
        return response.data;
    },

    async assignTicket(id: string, agentId: string): Promise<Ticket> {
        const response = await api.patch<Ticket>(`/api/tickets/${id}/assign`, { agentId });
        return response.data;
    },

    async addComment(ticketId: string, content: string): Promise<TicketComment> {
        const response = await api.post<TicketComment>(`/api/tickets/${ticketId}/comments`, { content });
        return response.data;
    },

    async deleteTicket(id: string): Promise<void> {
        await api.delete(`/api/tickets/${id}`);
    },

    async getStatistics(): Promise<{ byStatus: Record<string, number>; byPriority: Record<string, number>; total: number; unassigned: number }> {
        const response = await api.get('/api/tickets/statistics');
        return response.data;
    },
};
