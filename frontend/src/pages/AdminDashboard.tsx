import { Card, CardHeader, CardContent, CardDescription, CardTitle} from '@/components/ui/card';
import { Badge} from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import Layout from '@/components/ui/Layout';
import { mockUsers, mockTickets } from '@/data/mockData';
import { Link } from 'react-router-dom';
import { Users, Ticket, TrendingUp, AlertCircle, CheckCircle, Clock} from 'lucide-react';


export default function AdminDashboard(){
 
    const stats = {
        totalUsers: mockUsers.length,
        activeUsers: mockUsers.filter( u => u.isActive).length,
        totalTickets: mockTickets.length,
        openTickets: mockTickets.filter(t => t.status === 'Open').length,
        inProgressTickets: mockTickets.filter(t => t.status === 'InProgres').length,
        resolvedTickets: mockTickets.filter(t => t.status === 'Resolved').length,
        criticalTickets: mockTickets.filter( t => t.status === 'Critical').length,
        unassignedTickets: mockTickets.filter(t => t.status === 'unassigned').length,
        agents: mockUsers.filter( u => u.role === 'Agent').length,
        customers: mockUsers.filter( u=> u.role === 'Customer').length,
    };

    const recentActivity = [
        { id: 1, action: 'Nowy użytkownik zarejestrowany', user: 'Jan Kowalski', time: '5 min temu', type: 'user' },
        { id: 2, action: 'Zgłoszenie rozwiązane', ticket: '#1234', agent: 'Anna Nowak', time: '15 min temu', type: 'ticket' },
        { id: 3, action: 'Krytyczne zgłoszenie utworzone', ticket: '#1235', time: '30 min temu', type: 'alert' },
        { id: 4, action: 'Agent przypisany do zgłoszenia', ticket: '#1236', agent: 'Piotr Wiśniewski', time: '1 godz. temu', type: 'ticket' },
    ];

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
                                        ({((stats.openTickets / stats.totalTickets) * 100).toFixed(0)}%)
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
                                        ({((stats.inProgressTickets / stats.totalTickets) * 100).toFixed(0)}%)
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
                                        ({((stats.resolvedTickets / stats.totalTickets) * 100).toFixed(0)}%)
                                    </span>
                                </div>
                            </div>

                            <div className="pt-4 border-t">
                                <div className="w-full bg-gray-200 rounded-full h-2.5">
                                    <div 
                                        className="bg-green-500 h-2.5 rounded-full" 
                                        style={{ width: `${(stats.resolvedTickets / stats.totalTickets) * 100}%` }}
                                    ></div>
                                </div>
                                <p className="text-xs text-gray-500 mt-2 text-center">
                                    Skuteczność rozwiązywania: {((stats.resolvedTickets / stats.totalTickets) * 100).toFixed(1)}%
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
                                <span className="font-semibold">1</span>
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
                                <CardTitle>Ostatnia aktywność</CardTitle>
                                <CardDescription>Najnowsze zdarzenia w systemie</CardDescription>
                            </div>
                            <Button variant="outline" size="sm">
                                Zobacz wszystkie
                            </Button>
                        </div>
                    </CardHeader>
                    <CardContent>
                        <div className="space-y-4">
                            {recentActivity.map((activity) => (
                                <div key={activity.id} className="flex items-start gap-4 pb-4 border-b last:border-0 last:pb-0">
                                    <div className={`p-2 rounded-full ${
                                        activity.type === 'alert' ? 'bg-red-100' :
                                        activity.type === 'user' ? 'bg-blue-100' : 'bg-green-100'
                                    }`}>
                                        {activity.type === 'alert' ? (
                                            <AlertCircle className="h-4 w-4 text-red-600" />
                                        ) : activity.type === 'user' ? (
                                            <Users className="h-4 w-4 text-blue-600" />
                                        ) : (
                                            <CheckCircle className="h-4 w-4 text-green-600" />
                                        )}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <p className="text-sm font-medium text-gray-900">
                                            {activity.action}
                                        </p>
                                        <p className="text-sm text-gray-500">
                                            {activity.user && `${activity.user} • `}
                                            {activity.agent && `${activity.agent} • `}
                                            {activity.ticket && `${activity.ticket} • `}
                                            {activity.time}
                                        </p>
                                    </div>
                                </div>
                            ))}
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