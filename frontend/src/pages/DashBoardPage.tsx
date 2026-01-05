import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Link, useNavigate } from 'react-router-dom';
import Layout from '@/components/ui/Layout';
import { useEffect, useState } from 'react';
import { ticketService } from '@/services/ticketService';
import { Loader2 } from 'lucide-react';
import type { Ticket } from '@/types/ticket.types';

export default function DashboardPage() {
    const navigate = useNavigate();
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const role = user.role as 'Customer' | 'Agent' | 'Administrator';
    
    const [stats, setStats] = useState({
        total: 0,
        open: 0,
        inProgress: 0,
        resolved: 0,
    });
    const [recentTickets, setRecentTickets] = useState<Ticket[]>([]);
    const [loading, setLoading] = useState(true);
    const [ticketsLoading, setTicketsLoading] = useState(true);

    useEffect(() => {
        const loadDashboardData = async () => {
            try {
                setLoading(true);
                setTicketsLoading(true);

                // Load statistics
                const statsData = await ticketService.getStatistics();
                setStats({
                    total: statsData.total,
                    open: (statsData.byStatus['New'] || 0) + (statsData.byStatus['Open'] || 0),
                    inProgress: statsData.byStatus['InProgress'] || 0,
                    resolved: statsData.byStatus['Resolved'] || 0,
                });
                setLoading(false);

                // Load recent tickets based on user role
                let ticketsResponse: any;
                if (role === 'Customer') {
                    ticketsResponse = await ticketService.getMyTickets();
                } else {
                    ticketsResponse = await ticketService.getAllTickets();
                }

                // Handle different response formats from API
                const ticketsData = ticketsResponse.tickets || ticketsResponse.items || ticketsResponse;
                
                if (Array.isArray(ticketsData)) {
                    // Take only the first 5 tickets for recent view
                    setRecentTickets(ticketsData.slice(0, 5));
                } else {
                    console.error('Unexpected tickets response format:', ticketsResponse);
                    setRecentTickets([]);
                }
            } catch (error) {
                console.error('Failed to load dashboard data:', error);
            } finally {
                setLoading(false);
                setTicketsLoading(false);
            }
        };

        loadDashboardData();
    }, [role]);

    const getStatusVariant = (status: string) => {
        switch (status) {
            case 'Open':
            case 'New':
                return 'default';
            case 'InProgress':
                return 'secondary';
            case 'Resolved':
                return 'outline';
            default:
                return 'default';
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

    const getPriorityColor = (priority: string) => {
        switch (priority) {
            case 'Critical':
                return 'text-red-600';
            case 'High':
                return 'text-orange-600';
            case 'Medium':
                return 'text-yellow-600';
            case 'Low':
                return 'text-green-600';
            default:
                return 'text-grey-600';
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
        <Layout currentPage="/dashboard">
            {loading ? (
                <div className="flex justify-center items-center h-64">
                    <Loader2 className="h-8 w-8 animate-spin" />
                </div>
            ) : (
                <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8'>
                    <Card>
                        <CardHeader className='pb-3'>
                            <CardDescription>Wszystkie zgłoszenia</CardDescription>
                            <CardTitle className='text-3xl'>{stats.total}</CardTitle>
                        </CardHeader>
                    </Card>
                    <Card>
                        <CardHeader className='pb-3'>
                            <CardDescription>Otwarte</CardDescription>
                            <CardTitle className='text-3xl text-blue-600'>{stats.open}</CardTitle>
                        </CardHeader>
                    </Card>
                    <Card>
                        <CardHeader className='pb-3'>
                            <CardDescription>W trakcie</CardDescription>
                            <CardTitle className='text-3xl text-orange-600'>{stats.inProgress}</CardTitle>
                        </CardHeader>
                    </Card>
                    <Card>
                        <CardHeader className='pb-3'>
                            <CardDescription>Rozwiązane</CardDescription>
                            <CardTitle className='text-3xl text-green-600'>{stats.resolved}</CardTitle>
                        </CardHeader>
                    </Card>
                </div>
            )}

            <Card>
                <CardHeader>
                    <div className='flex justify-between items-center'>
                        <div>
                            <CardTitle>Ostatnie zgłoszenia</CardTitle>
                            <CardDescription>Przegląd najnowszych zgłoszeń</CardDescription>
                        </div>
                        <Link to="/tickets">
                            <Button>Zobacz wszystkie</Button>
                        </Link>
                    </div>
                </CardHeader>
                <CardContent>
                    {ticketsLoading ? (
                        <div className="flex justify-center items-center py-12">
                            <Loader2 className="h-6 w-6 animate-spin" />
                            <span className="ml-2 text-gray-500">Ładowanie zgłoszeń...</span>
                        </div>
                    ) : (
                        <div className='space-y-4'>
                            {recentTickets.map((ticket) => (
                                <Card 
                                    key={ticket.id} 
                                    className='hover:shadow-lg transition-shadow cursor-pointer'
                                    onClick={() => navigate(`/tickets/${ticket.id}`)}
                                >
                                    <CardHeader>
                                        <div className="flex justify-between items-start">
                                            <div className='flex-1'>
                                                <div className='flex items-center gap-3 mb-2 flex-wrap'>
                                                    <CardTitle className='text-lg'>{ticket.title}</CardTitle>
                                                    <Badge variant={getStatusVariant(ticket.status)}>
                                                        {getStatusLabel(ticket.status)}
                                                    </Badge>
                                                    <span className={`text-sm font-semibold ${getPriorityColor(ticket.priority)}`}>
                                                        {getPriorityLabel(ticket.priority)}
                                                    </span>
                                                </div>
                                                <CardDescription className='line-clamp-2'>{ticket.description}</CardDescription>
                                            </div>
                                        </div>
                                    </CardHeader>
                                    <CardContent>
                                        <div className='flex justify-between items-center text-sm text-gray-600'>
                                            <div>
                                                {ticket.agentName || ticket.assignedAgentName ? (
                                                    <span>Przypisany do: <span className='font-semibold'>{ticket.agentName || ticket.assignedAgentName}</span></span>
                                                ) : (
                                                    <span className='text-gray-400'>Nieprzypisany</span>
                                                )}
                                            </div>
                                            <div>
                                                {new Date(ticket.createdAt).toLocaleDateString('pl-PL', {
                                                    day: 'numeric',
                                                    month: 'long',
                                                    year: 'numeric',
                                                })}
                                            </div>
                                        </div>
                                    </CardContent>
                                </Card>
                            ))}
                            {recentTickets.length === 0 && (
                                <Card className='text-center py-12'>
                                    <CardContent>
                                        <p className='text-gray-500 mb-4'>Brak zgłoszeń do wyświetlenia</p>
                                        <Button onClick={() => navigate('/tickets/create')}>Utwórz nowe zgłoszenie</Button>
                                    </CardContent>
                                </Card>
                            )}
                        </div>
                    )}
                </CardContent>
            </Card>
        </Layout>
    );
}
