import { useState, useEffect, useCallback } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Tabs } from '@/components/ui/tabs';
import { useNavigate } from 'react-router-dom';
import Layout from '@/components/ui/Layout';
import { ticketService } from '@/services/ticketService';
import TicketFilters from '@/components/TicketFilters';
import Pagination from '@/components/Pagination';
import type { Ticket, TicketSearchFilter } from '@/types/ticket.types';
import { Loader2, AlertCircle, RefreshCw } from 'lucide-react';

export default function TicketsPage() {
  const navigate = useNavigate();
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Tab state (for agents/admins)
  const [activeTab, setActiveTab] = useState<'all' | 'assigned'>('all');

  // Filters and pagination
  const [filters, setFilters] = useState<TicketSearchFilter>({
    searchTerm: '',
    status: '',
    priority: '',
    category: '',
    page: 1,
    pageSize: 10
  });
  const [totalCount, setTotalCount] = useState(0);

  const user = JSON.parse(localStorage.getItem('user') || '{}');
  const isCustomer = user.role === 'Customer';
  const isAgentOrAdmin = user.role === 'Agent' || user.role === 'Administrator';
  const isAdmin = user.role === "Admin";

  const totalPages = Math.ceil(totalCount / (filters.pageSize || 10));

  const loadTickets = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      if (isCustomer) {
        // Customer: use simple getMyTickets
        const response = await ticketService.getMyTickets();
        const data = response.tickets || response;
        setTickets(Array.isArray(data) ? data : []);
        setTotalCount(response.totalCount || (Array.isArray(data) ? data.length : 0));
      } else if (activeTab === 'assigned') {
        // Agent/Admin: assigned tab
        const response = await ticketService.getAssignedTickets(
          filters.page || 1,
          filters.pageSize || 10
        );
        setTickets(response.tickets || []);
        setTotalCount(response.totalCount || 0);
      } else {
        // Agent/Admin: all tab with server-side search
        const searchFilters: TicketSearchFilter = {
          page: filters.page || 1,
          pageSize: filters.pageSize || 10,
        };

        // Only add non-empty filters
        if (filters.searchTerm) searchFilters.searchTerm = filters.searchTerm;
        if (filters.status && filters.status !== 'all') searchFilters.status = filters.status;
        if (filters.priority && filters.priority !== 'all') searchFilters.priority = filters.priority;
        if (filters.category && filters.category !== 'all') searchFilters.category = filters.category;

        const response = await ticketService.searchTickets(searchFilters);
        setTickets(response.tickets || []);
        setTotalCount(response.totalCount || 0);
      }
    } catch (err: any) {
      console.error("Failed to load tickets:", err);
      setError(err.response?.data?.message || 'Nie udało się załadować zgłoszeń. Spróbuj ponownie.');
      setTickets([]);
    } finally {
      setLoading(false);
    }
  }, [isCustomer, activeTab, filters.page, filters.pageSize, filters.status, filters.priority, filters.category, filters.searchTerm]);

  // Reload on filter/tab changes
  useEffect(() => {
    loadTickets();
  }, [loadTickets]);

  // Debounced search term
  useEffect(() => {
    if (isCustomer || activeTab === 'assigned') return;

    const timer = setTimeout(() => {
      loadTickets();
    }, 500);

    return () => clearTimeout(timer);
  }, [filters.searchTerm]);

  const handleTabChange = (tabId: string) => {
    setActiveTab(tabId as 'all' | 'assigned');
    setFilters(prev => ({ ...prev, page: 1 }));
  };

  const handleFiltersChange = (newFilters: TicketSearchFilter) => {
    setFilters(newFilters);
  };

  const handlePageChange = (page: number) => {
    setFilters(prev => ({ ...prev, page }));
  };

  const getStatusVariant = (status: string) => {
    switch (status) {
      case 'Open':
      case 'New':
        return 'default';
      case 'Resolved':
        return 'outline';
      default:
        return 'secondary';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'Open':
      case 'New':
        return 'Otwarte';
      case 'InProgress':
        return 'W trakcie';
      case 'Resolved':
        return 'Rozwiązane';
      case 'Closed':
        return 'Zamknięte';
      case 'Assigned':
        return 'Przypisane';
      case 'Pending':
        return 'Oczekujące';
      default:
        return status;
    }
  };

  const getPriorityLabel = (priority: string) => {
    switch (priority) {
      case 'Critical':
        return 'Krytyczny';
      case 'High':
        return 'Wysoki';
      case 'Medium':
        return 'Średni';
      case 'Low':
        return 'Niski';
      default:
        return priority;
    }
  };

  const tabs = [
    { id: 'all', label: 'Wszystkie' },
    { id: 'assigned', label: 'Przypisane do mnie' }
  ];

  return (
    <Layout currentPage="/tickets">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Zgłoszenia</h1>
          <p className="text-gray-600 mt-1">
            {loading ? 'Ładowanie...' : `Znaleziono ${totalCount} zgłoszeń`}
          </p>
        </div>
        {isCustomer &&(
        <Button onClick={() => navigate('/tickets/create')}>
          + Nowe zgłoszenie
        </Button>
        )}
      </div>

      {/* Tabs for Agent/Admin */}
      {isAgentOrAdmin && (
        <Tabs
          tabs={tabs}
          activeTab={activeTab}
          onTabChange={handleTabChange}
          className="mb-4"
        />
      )}

      {/* Filters - only for "all" tab and Agent/Admin */}
      {isAgentOrAdmin && activeTab === 'all' && (
        <Card className="mb-6">
          <CardContent className="pt-6">
            <TicketFilters
              filters={filters}
              onFiltersChange={handleFiltersChange}
              loading={loading}
            />
          </CardContent>
        </Card>
      )}

      {loading ? (
        <div className="flex justify-center items-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
          <span className="ml-2 text-gray-600">Ładowanie zgłoszeń...</span>
        </div>
      ) : error ? (
        <Card className="text-center py-12">
          <CardContent>
            <AlertCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
            <p className="text-red-600 mb-4">{error}</p>
            <Button onClick={loadTickets} variant="outline">
              <RefreshCw className="mr-2 h-4 w-4" />
              Spróbuj ponownie
            </Button>
          </CardContent>
        </Card>
      ) : tickets.length === 0 ? (
        <Card className="text-center py-12">
          <CardContent>
            <p className="text-gray-500 text-lg mb-4">
              {activeTab === 'assigned'
                ? 'Nie masz przypisanych zgłoszeń'
                : filters.searchTerm || filters.status || filters.priority || filters.category
                  ? 'Nie znaleziono zgłoszeń pasujących do filtrów'
                  : 'Brak zgłoszeń'
              }
            </p>
            {!isCustomer && activeTab === 'all' && !filters.searchTerm && !filters.status && !filters.priority && !filters.category && (
              <Button onClick={() => navigate('/tickets/create')}>
                Utwórz pierwsze zgłoszenie
              </Button>
            )}
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4">
            {tickets.map((ticket) => (
              <Card
                key={ticket.id}
                className="hover:shadow-lg transition-shadow cursor-pointer"
                onClick={() => navigate(`/tickets/${ticket.id}`)}
              >
                <CardHeader>
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2 flex-wrap">
                        <CardTitle className="text-lg">{ticket.title}</CardTitle>
                        <Badge variant={getStatusVariant(ticket.status)}>
                          {getStatusLabel(ticket.status)}
                        </Badge>
                        <Badge variant="outline">{getPriorityLabel(ticket.priority)}</Badge>
                      </div>
                      <CardDescription className="line-clamp-2">
                        {ticket.description}
                      </CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="flex justify-between items-center text-sm text-gray-600">
                    <div>
                      {ticket.agentName || ticket.assignedAgentName ? (
                        <span>Przypisany do: <span className="font-semibold">{ticket.agentName || ticket.assignedAgentName}</span></span>
                      ) : (
                        <span className="text-gray-400">Nieprzypisany</span>
                      )}
                    </div>
                    <span>Utworzono: {new Date(ticket.createdAt).toLocaleDateString('pl-PL')}</span>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Pagination */}
          <Pagination
            currentPage={filters.page || 1}
            totalPages={totalPages}
            totalCount={totalCount}
            pageSize={filters.pageSize || 10}
            onPageChange={handlePageChange}
            loading={loading}
          />
        </>
      )}
    </Layout>
  );
}
