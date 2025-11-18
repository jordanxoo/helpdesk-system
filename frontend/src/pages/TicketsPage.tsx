import { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { mockTickets } from '@/data/mockData';
import { Link, useNavigate } from 'react-router-dom';
import Layout from '@/components/ui/Layout';

export default function TicketsPage() {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [priorityFilter, setPriorityFilter] = useState<string>('all');

  const filteredTickets = mockTickets.filter(ticket => {
    const matchesSearch = ticket.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         ticket.description.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === 'all' || ticket.status === statusFilter;
    const matchesPriority = priorityFilter === 'all' || ticket.priority === priorityFilter;
    
    return matchesSearch && matchesStatus && matchesPriority;
  });

  const getStatusVariant = (status: string) => {
    switch (status) {
      case 'Open':
        return 'default';
      case 'InProgress':
        return 'secondary';
      case 'Resolved':
        return 'outline';
      default:
        return 'default';
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'Critical':
        return 'text-red-600 bg-red-50 border-red-200';
      case 'High':
        return 'text-orange-600 bg-orange-50 border-orange-200';
      case 'Medium':
        return 'text-yellow-600 bg-yellow-50 border-yellow-200';
      case 'Low':
        return 'text-green-600 bg-green-50 border-green-200';
      default:
        return 'text-gray-600 bg-gray-50 border-gray-200';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'Open':
        return 'Otwarte';
      case 'InProgress':
        return 'W trakcie';
      case 'Resolved':
        return 'Rozwiązane';
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
            <h1 className="text-3xl font-bold text-slate-900">Wszystkie zgłoszenia</h1>
            <p className="text-gray-600 mt-1">
              Znaleziono {filteredTickets.length} zgłoszeń
            </p>
          </div>
          <Button>
            <Link to="/tickets/create">+ Nowe zgłoszenie</Link>
          </Button>
        </div>

        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Filtry</CardTitle>
            <CardDescription>Wyszukaj i filtruj zgłoszenia</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Wyszukaj
                </label>
                <Input
                  type="text"
                  placeholder="Szukaj po tytule lub opisie..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Status
                </label>
                <Select value={statusFilter} onValueChange={setStatusFilter}>
                  <SelectTrigger>
                    <SelectValue placeholder="Wszystkie" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">Wszystkie</SelectItem>
                    <SelectItem value="Open">Otwarte</SelectItem>
                    <SelectItem value="InProgress">W trakcie</SelectItem>
                    <SelectItem value="Resolved">Rozwiązane</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Priorytet
                </label>
                <Select value={priorityFilter} onValueChange={setPriorityFilter}>
                  <SelectTrigger>
                    <SelectValue placeholder="Wszystkie" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">Wszystkie</SelectItem>
                    <SelectItem value="Critical">Krytyczny</SelectItem>
                    <SelectItem value="High">Wysoki</SelectItem>
                    <SelectItem value="Medium">Średni</SelectItem>
                    <SelectItem value="Low">Niski</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </CardContent>
        </Card>

        {filteredTickets.length === 0 ? (
          <Card className="text-center py-12">
            <CardContent>
              <p className="text-gray-500 text-lg mb-4">
                Nie znaleziono zgłoszeń pasujących do kryteriów
              </p>
              <Button variant="outline" onClick={() => {
                setSearchQuery('');
                setStatusFilter('all');
                setPriorityFilter('all');
              }}>
                Wyczyść filtry
              </Button>
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
                        <span className={`text-xs font-semibold px-2 py-1 rounded-md border ${getPriorityColor(ticket.priority)}`}>
                          {getPriorityLabel(ticket.priority)}
                        </span>
                      </div>
                      <CardDescription className="line-clamp-2">
                        {ticket.description}
                      </CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="flex justify-between items-center text-sm text-gray-600">
                    <div className="flex items-center gap-4">
                      <div>
                        {ticket.agentName ? (
                          <span>
                            Przypisany do:{' '}
                            <span className="font-semibold text-gray-900">
                              {ticket.agentName}
                            </span>
                          </span>
                        ) : (
                          <span className="text-gray-400">Nieprzypisany</span>
                        )}
                      </div>
                      {ticket.category && (
                        <span className="text-xs bg-gray-100 px-2 py-1 rounded-md">
                          {ticket.category}
                        </span>
                      )}
                    </div>
                    <div className="text-right">
                      <div>
                        Utworzono:{' '}
                        {new Date(ticket.createdAt).toLocaleDateString('pl-PL', {
                          day: 'numeric',
                          month: 'long',
                          year: 'numeric',
                        })}
                      </div>
                      {ticket.updatedAt && (
                        <div className="text-xs text-gray-500">
                          Zaktualizowano:{' '}
                          {new Date(ticket.updatedAt).toLocaleDateString('pl-PL')}
                        </div>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </Layout>
    );
}