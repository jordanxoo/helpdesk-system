import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {useNavigate } from 'react-router-dom';
import Layout from '@/components/ui/Layout';
import { ticketService } from '@/services/ticketService';
import type { Ticket } from '@/types/ticket.types';

export default function TicketsPage() {
  const navigate = useNavigate();
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');

  const user = JSON.parse(localStorage.getItem('user') || '{}');

  useEffect(() => {
    loadTickets();
  }, []);

  const loadTickets = async () => {
    try {
      setLoading(true);
      let response: any;

      if (user.role === 'Customer') {
        response = await ticketService.getMyTickets();
      } else {
        response = await ticketService.getAllTickets();
      }

      // Obsługa różnych formatów odpowiedzi z API
      // Backend zwraca: { tickets: [...], totalCount: ... }
      const data = response.tickets || response.items || response;

      if (Array.isArray(data)) {
        setTickets(data);
      } else {
        console.error("API response is not an array:", response);
        setTickets([]);
      }
    } catch (error) {
      console.error("Failed to load tickets:", error);
    } finally {
      setLoading(false);
    }
  };

  const filteredTickets = tickets.filter(ticket => 
    ticket.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    ticket.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const getStatusVariant = (status: string) => {
    return status === 'Open' ? 'default' : status === 'Resolved' ? 'outline' : 'secondary';
  };

  return (
    <Layout currentPage="/tickets">
      <div className="flex justify-between items-center mb-6">
          <div>
            <h1 className="text-3xl font-bold text-slate-900">Zgłoszenia</h1>
            <p className="text-gray-600 mt-1">
              Znaleziono {filteredTickets.length} zgłoszeń
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
           <div className="text-center py-12">Ładowanie zgłoszeń...</div>
        ) : filteredTickets.length === 0 ? (
          <Card className="text-center py-12">
            <CardContent>
              <p className="text-gray-500 text-lg mb-4">Brak zgłoszeń</p>
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
                        <Badge variant={getStatusVariant(ticket.status)}>{ticket.status}</Badge>
                        <Badge variant="outline">{ticket.priority}</Badge>
                      </div>
                      <CardDescription className="line-clamp-2">
                        {ticket.description}
                      </CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="flex justify-between items-center text-sm text-gray-600">
                     <span>Utworzono: {new Date(ticket.createdAt).toLocaleDateString()}</span>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </Layout>
    );
}