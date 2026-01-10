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

export interface TicketAttachment {
  id: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  downloadUrl: string;
  uploadedAt: string;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: string;
  category: string;
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
    attachment?: TicketAttachment[];
}

// Search filter request (mirrors backend TicketFilterRequest)
export interface TicketSearchFilter {
  searchTerm?: string;
  status?: string;
  priority?: string;
  category?: string;
  customerId?: string;
  assignedAgentId?: string;
  page?: number;
  pageSize?: number;
}

// Paginated response from API
export interface PaginatedTicketResponse {
  tickets: Ticket[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Audit log entry (mirrors backend TicketAuditLogDto)
export interface TicketHistoryEntry {
  id: string;
  userId: string;
  action: string;
  fieldName?: string;
  oldValue?: string;
  newValue?: string;
  description?: string;
  createdAt: string;
}

// Update ticket request
export interface UpdateTicketRequest {
  title?: string;
  description?: string;
  category?: string;
  priority?: string;
}