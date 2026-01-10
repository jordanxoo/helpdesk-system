import api from './api';
import type {
    Ticket,
    CreateTicketRequest,
    TicketDetails,
    TicketAttachment,
    TicketSearchFilter,
    TicketHistoryEntry,
    UpdateTicketRequest,
    PaginatedTicketResponse
} from '../types/ticket.types';

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

    async updateTicketStatus(id: string, status: string): Promise<TicketDetails> {
        const response = await api.patch<TicketDetails>(`/api/tickets/${id}/status`, { newStatus: status });
        return response.data;
    },

    async updateTicketPriority(id: string, priority: string): Promise<TicketDetails> {
        const response = await api.patch<TicketDetails>(`/api/tickets/${id}/priority`, { newPriority: priority });
        return response.data;
    },

    async assignTicket(id: string, agentId: string): Promise<TicketDetails> {
        // Send null for unassign (empty string), or the agentId for assign
        const agentIdValue = agentId && agentId.trim() !== '' ? agentId : null;
        const response = await api.patch<TicketDetails>(`/api/tickets/${id}/assign`, { agentId: agentIdValue });
        return response.data;
    },

    async addComment(ticketId: string, content: string): Promise<TicketDetails> {
        const response = await api.post<TicketDetails>(`/api/tickets/${ticketId}/comments`, { content });
        return response.data;
    },

    async closeTicket(id: string): Promise<TicketDetails> {
        const response = await api.patch<TicketDetails>(`/api/tickets/${id}/close`);
        return response.data;
    },

    async deleteTicket(id: string): Promise<void> {
        await api.delete(`/api/tickets/${id}`);
    },

    async getStatistics(): Promise<{ byStatus: Record<string, number>; byPriority: Record<string, number>; total: number; unassigned: number }> {
        const response = await api.get('/api/tickets/statistics');
        return response.data;
    },

    async uploadAttachment(ticketId: string, file: File): Promise<TicketAttachment> {
        const formData = new FormData();
        formData.append('file', file);

        const response = await api.post<TicketAttachment>(
            `/api/tickets/${ticketId}/attachments`,
            formData,
            {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            }
        );
        return response.data;
    },

    async searchTickets(filter: TicketSearchFilter): Promise<PaginatedTicketResponse> {
        const response = await api.post<PaginatedTicketResponse>('/api/tickets/search', filter);
        return response.data;
    },

    async getAssignedTickets(page: number = 1, pageSize: number = 10): Promise<PaginatedTicketResponse> {
        const response = await api.get<PaginatedTicketResponse>('/api/tickets/assigned', {
            params: { page, pageSize }
        });
        return response.data;
    },

    async updateTicket(id: string, data: UpdateTicketRequest): Promise<TicketDetails> {
        const response = await api.put<TicketDetails>(`/api/tickets/${id}`, data);
        return response.data;
    },

    async getTicketHistory(ticketId: string): Promise<TicketHistoryEntry[]> {
        const response = await api.get<TicketHistoryEntry[]>(`/api/tickets/${ticketId}/history`);
        return response.data;
    },
};
