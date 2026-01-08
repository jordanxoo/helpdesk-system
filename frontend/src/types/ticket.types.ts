export interface Ticket {
  id: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  category?: string;
  customerId: string;
  customerName?: string;
  customerEmail?: string;
  assignedAgentId: string | null;  // Backend sends assignedAgentId
  agentId?: string | null;         // Alias for compatibility
  agentName?: string | null;
  assignedAgentName?: string | null;
  createdAt: string;
  updatedAt: string;
  comments: TicketComment[];
}

export interface TicketComment {
  id: string;
  ticketId: string;
  userId: string;
  userName: string;
  userRole?: string;
  content: string;
  createdAt: string;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: string;
}

export interface AddCommentRequest {
  content: string;
}

export interface TicketListResponse {
  items: Ticket[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface TicketDetails extends Ticket {
    comments: TicketComment[];
}