import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useNavigate } from 'react-router-dom';
import Layout from '@/components/ui/Layout';
import { ticketService } from '@/services/ticketService';
import type { Ticket } from '@/types/ticket.types';
import { Loader2, AlertCircle, RefreshCw } from 'lucide-react';

export default function TicketsPage() {
  const navigate = useNavigate();
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  const user = JSON.parse(localStorage.getItem('user') || '{}');

  useEffect(() => {
    loadTickets();
  }, []);

  const loadTickets = async () => {
    try {
      setLoading(true);
      setError(null);
      let response: any;

      if (user.role === 'Customer') {
        response = await ticketService.getMyTickets();
      } else {
        response = await ticketService.getAllTickets();
      }

      // Handle paginated response format from API
      // Backend returns: { tickets: [...], totalCount: ... }
      const data = response.tickets || response.items || response;

      if (Array.isArray(data)) {
        setTickets(data);
      } else {
        console.error("API response is not an array:", response);
        setTickets([]);
      }
    } catch (err: any) {
      console.error("Failed to load tickets:", err);
      setError(err.response?.data?.message || 'Nie udało się załadować zgłoszeń. Spróbuj ponownie.');
    } finally {
      setLoading(false);
    }
  };

  const filteredTickets = tickets.filter(ticket => 
    ticket.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    ticket.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

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

  return (
    <Layout currentPage="/tickets">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Zgłoszenia</h1>
          <p className="text-gray-600 mt-1">
            {loading ? 'Ładowanie...' : `Znaleziono ${filteredTickets.length} zgłoszeń`}
          </p>
        </div>
        <Button onClick={() => navigate('/tickets/create')}>
          + Nowe zgłoszenie
        </Button>
      </div>

      <Card className="mb-6">
        <CardContent className="pt-6">
          <Input
            type="text"
            placeholder="Szukaj po tytule lub opisie..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </CardContent>
      </Card>

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
      ) : filteredTickets.length === 0 ? (
        <Card className="text-center py-12">
          <CardContent>
            <p className="text-gray-500 text-lg mb-4">
              {searchQuery ? 'Nie znaleziono zgłoszeń pasujących do wyszukiwania' : 'Brak zgłoszeń'}
            </p>
            {!searchQuery && (
              <Button onClick={() => navigate('/tickets/create')}>
                Utwórz pierwsze zgłoszenie
              </Button>
            )}
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 gap-4">
          {filteredTickets.map((ticket) => (
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
      )}
    </Layout>
  );
}
