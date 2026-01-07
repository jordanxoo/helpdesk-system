import { useState, useEffect } from 'react';
import { Card, CardHeader, CardContent, CardDescription, CardTitle} from '@/components/ui/card';
import { Badge} from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import Layout from '@/components/ui/Layout';
import { Link, useNavigate } from 'react-router-dom';
import { Users, Ticket, TrendingUp, AlertCircle, Clock, Loader2 } from 'lucide-react';
import { userService } from '@/services/userService';
import { ticketService } from '@/services/ticketService';
import type { Ticket as TicketType } from '@/types/ticket.types';

interface Stats {
    totalUsers: number;
    activeUsers: number;
    totalTickets: number;
    openTickets: number;
    inProgressTickets: number;
    resolvedTickets: number;
    criticalTickets: number;
    unassignedTickets: number;
    agents: number;
    customers: number;
    admins: number;
}

export default function AdminDashboard(){
    const navigate = useNavigate();
    const [stats, setStats] = useState<Stats>({
        totalUsers: 0,
        activeUsers: 0,
        totalTickets: 0,
        openTickets: 0,
        inProgressTickets: 0,
        resolvedTickets: 0,
        criticalTickets: 0,
        unassignedTickets: 0,
        agents: 0,
        customers: 0,
        admins: 0,
    });
    const [recentTickets, setRecentTickets] = useState<TicketType[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadDashboardData();
    }, []);

    const loadDashboardData = async () => {
        try {
            setLoading(true);
            setError(null);

            // Fetch users, ticket statistics and recent tickets in parallel
            const [users, ticketStats, ticketsResponse] = await Promise.all([
                userService.getAllUsers(),
                ticketService.getStatistics(),
                ticketService.getAllTickets()
            ]);

            // Calculate user stats
            const activeUsers = users.filter(u => u.isActive).length;
            const agents = users.filter(u => u.role === 'Agent').length;
            const customers = users.filter(u => u.role === 'Customer').length;
            const admins = users.filter(u => u.role === 'Administrator').length;

            // Calculate ticket stats from API response
            const openTickets = (ticketStats.byStatus['New'] || 0) + (ticketStats.byStatus['Open'] || 0);
            const inProgressTickets = ticketStats.byStatus['InProgress'] || 0;
            const resolvedTickets = ticketStats.byStatus['Resolved'] || 0;
            const criticalTickets = ticketStats.byPriority['Critical'] || 0;

            // Get recent tickets
            const ticketsData = ticketsResponse.tickets || ticketsResponse;
            if (Array.isArray(ticketsData)) {
                setRecentTickets(ticketsData.slice(0, 5));
            }

            setStats({
                totalUsers: users.length,
                activeUsers,
                totalTickets: ticketStats.total,
                openTickets,
                inProgressTickets,
                resolvedTickets,
                criticalTickets,
                unassignedTickets: ticketStats.unassigned,
                agents,
                customers,
                admins,
            });
        } catch (err) {
            console.error('Failed to load dashboard data:', err);
            setError('Nie udało się załadować danych. Spróbuj ponownie.');
        } finally {
            setLoading(false);
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
                return 'text-gray-600';
        }
    };

    // Helper function to safely calculate percentage
    const calculatePercentage = (part: number, total: number): string => {
        if (total === 0) return '0';
        return ((part / total) * 100).toFixed(0);
    };

    if (loading) {
        return (
            <Layout currentPage="/admin">
                <div className="flex justify-center items-center h-64">
                    <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
                    <span className="ml-2 text-gray-600">Ładowanie danych...</span>
                </div>
            </Layout>
        );
    }

    if (error) {
        return (
            <Layout currentPage="/admin">
                <div className="flex flex-col justify-center items-center h-64">
                    <AlertCircle className="h-12 w-12 text-red-500 mb-4" />
                    <p className="text-red-600 mb-4">{error}</p>
                    <Button onClick={loadDashboardData}>Spróbuj ponownie</Button>
                </div>
            </Layout>
        );
    }

    return (
        <Layout currentPage="/admin">
            <div className="space-y-6">
                
                <div className="flex justify-between items-center">
                    <div>
                        <h1 className="text-3xl font-bold text-slate-900">Panel Administratora</h1>
                        <p className="text-gray-600 mt-1">Przegląd systemu i statystyki</p>
                    </div>
                    <Badge className="bg-red-500 text-white">Administrator</Badge>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between pb-2">
                            <CardTitle className="text-sm font-medium text-gray-600">
                                Użytkownicy
                            </CardTitle>
                            <Users className="h-4 w-4 text-blue-600" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold">{stats.totalUsers}</div>
                            <p className="text-xs text-gray-500 mt-1">
                                {stats.activeUsers} aktywnych
                            </p>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between pb-2">
                            <CardTitle className="text-sm font-medium text-gray-600">
                                Wszystkie zgłoszenia
                            </CardTitle>
                            <Ticket className="h-4 w-4 text-purple-600" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold">{stats.totalTickets}</div>
                            <p className="text-xs text-gray-500 mt-1">
                                {stats.openTickets} otwartych
                            </p>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between pb-2">
                            <CardTitle className="text-sm font-medium text-gray-600">
                                Krytyczne
                            </CardTitle>
                            <AlertCircle className="h-4 w-4 text-red-600" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold text-red-600">{stats.criticalTickets}</div>
                            <p className="text-xs text-gray-500 mt-1">
                                wymagają uwagi
                            </p>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between pb-2">
                            <CardTitle className="text-sm font-medium text-gray-600">
                                Nieprzypisane
                            </CardTitle>
                            <Clock className="h-4 w-4 text-orange-600" />
                        </CardHeader>
                        <CardContent>
                            <div className="text-2xl font-bold text-orange-600">{stats.unassignedTickets}</div>
                            <p className="text-xs text-gray-500 mt-1">
                                oczekują na agenta
                            </p>
                        </CardContent>
                    </Card>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <Card>
                        <CardHeader>
                            <CardTitle>Status zgłoszeń</CardTitle>
                            <CardDescription>Podział według statusu</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2">
                                    <div className="w-3 h-3 bg-blue-500 rounded-full"></div>
                                    <span className="text-sm">Otwarte</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <span className="font-semibold">{stats.openTickets}</span>
                                    <span className="text-xs text-gray-500">
                                        ({calculatePercentage(stats.openTickets, stats.totalTickets)}%)
                                    </span>
                                </div>
                            </div>
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2">
                                    <div className="w-3 h-3 bg-orange-500 rounded-full"></div>
                                    <span className="text-sm">W trakcie</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <span className="font-semibold">{stats.inProgressTickets}</span>
                                    <span className="text-xs text-gray-500">
                                        ({calculatePercentage(stats.inProgressTickets, stats.totalTickets)}%)
                                    </span>
                                </div>
                            </div>
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2">
                                    <div className="w-3 h-3 bg-green-500 rounded-full"></div>
                                    <span className="text-sm">Rozwiązane</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <span className="font-semibold">{stats.resolvedTickets}</span>
                                    <span className="text-xs text-gray-500">
                                        ({calculatePercentage(stats.resolvedTickets, stats.totalTickets)}%)
                                    </span>
                                </div>
                            </div>

                            <div className="pt-4 border-t">
                                <div className="w-full bg-gray-200 rounded-full h-2.5">
                                    <div 
                                        className="bg-green-500 h-2.5 rounded-full" 
                                        style={{ width: `${stats.totalTickets > 0 ? (stats.resolvedTickets / stats.totalTickets) * 100 : 0}%` }}
                                    ></div>
                                </div>
                                <p className="text-xs text-gray-500 mt-2 text-center">
                                    Skuteczność rozwiązywania: {stats.totalTickets > 0 ? ((stats.resolvedTickets / stats.totalTickets) * 100).toFixed(1) : '0.0'}%
                                </p>
                            </div>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle>Użytkownicy systemu</CardTitle>
                            <CardDescription>Podział według ról</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2">
                                    <Badge className="bg-red-500 text-white">Administrator</Badge>
                                </div>
                                <span className="font-semibold">{stats.admins}</span>
                            </div>
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2">
                                    <Badge className="bg-blue-500 text-white">Agent</Badge>
                                </div>
                                <span className="font-semibold">{stats.agents}</span>
                            </div>
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2">
                                    <Badge className="bg-green-500 text-white">Klient</Badge>
                                </div>
                                <span className="font-semibold">{stats.customers}</span>
                            </div>

                            <div className="pt-4 border-t space-y-2">
                                <Link to="/users">
                                    <Button className="w-full" variant="outline">
                                        <Users className="mr-2 h-4 w-4" />
                                        Zarządzaj użytkownikami
                                    </Button>
                                </Link>
                                <Link to="/users/create">
                                    <Button className="w-full">
                                        Dodaj użytkownika
                                    </Button>
                                </Link>
                            </div>
                        </CardContent>
                    </Card>
                </div>

                <Card>
                    <CardHeader>
                        <div className="flex justify-between items-center">
                            <div>
                                <CardTitle>Ostatnie zgłoszenia</CardTitle>
                                <CardDescription>Najnowsze zgłoszenia w systemie</CardDescription>
                            </div>
                            <Link to="/tickets">
                                <Button variant="outline" size="sm">
                                    Zobacz wszystkie
                                </Button>
                            </Link>
                        </div>
                    </CardHeader>
                    <CardContent>
                        <div className="space-y-4">
                            {recentTickets.length > 0 ? (
                                recentTickets.map((ticket) => (
                                    <div 
                                        key={ticket.id} 
                                        className="flex items-start gap-4 pb-4 border-b last:border-0 last:pb-0 cursor-pointer hover:bg-gray-50 -mx-2 px-2 py-2 rounded"
                                        onClick={() => navigate(`/tickets/${ticket.id}`)}
                                    >
                                        <div className={`p-2 rounded-full ${
                                            ticket.priority === 'Critical' ? 'bg-red-100' :
                                            ticket.priority === 'High' ? 'bg-orange-100' : 'bg-blue-100'
                                        }`}>
                                            {ticket.priority === 'Critical' ? (
                                                <AlertCircle className="h-4 w-4 text-red-600" />
                                            ) : (
                                                <Ticket className="h-4 w-4 text-blue-600" />
                                            )}
                                        </div>
                                        <div className="flex-1 min-w-0">
                                            <p className="text-sm font-medium text-gray-900 truncate">
                                                {ticket.title}
                                            </p>
                                            <p className="text-sm text-gray-500">
                                                {ticket.customerName} • {getStatusLabel(ticket.status)} • 
                                                <span className={getPriorityColor(ticket.priority)}> {ticket.priority}</span> • 
                                                {new Date(ticket.createdAt).toLocaleDateString('pl-PL')}
                                            </p>
                                        </div>
                                    </div>
                                ))
                            ) : (
                                <p className="text-center text-gray-500 py-4">Brak zgłoszeń</p>
                            )}
                        </div>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader>
                        <CardTitle>Szybkie akcje</CardTitle>
                        <CardDescription>Najczęściej używane funkcje</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                            <Link to="/tickets">
                                <Button variant="outline" className="w-full h-auto py-4 flex flex-col items-center gap-2">
                                    <Ticket className="h-6 w-6" />
                                    <span>Wszystkie zgłoszenia</span>
                                </Button>
                            </Link>
                            <Link to="/users">
                                <Button variant="outline" className="w-full h-auto py-4 flex flex-col items-center gap-2">
                                    <Users className="h-6 w-6" />
                                    <span>Zarządzanie użytkownikami</span>
                                </Button>
                            </Link>
                            <Button variant="outline" className="w-full h-auto py-4 flex flex-col items-center gap-2">
                                <TrendingUp className="h-6 w-6" />
                                <span>Raporty i statystyki</span>
                            </Button>
                        </div>
                    </CardContent>
                </Card>
            </div>
        </Layout>
    );
}
