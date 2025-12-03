import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { mockStats, mockTickets } from '@/data/mockData';
import { Link, useNavigate } from 'react-router-dom';
import Layout from '@/components/ui/Layout';
import { useEffect } from 'react';

export default function DashboardPage() {
    const navigate = useNavigate();
    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const role = user.role as 'Customer' | 'Agent' | 'Administrator';

    useEffect(() => {
        if (role === 'Administrator') {
            navigate('/admin');
        }
    }, [role, navigate]);

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

    return (
        <Layout currentPage="/dashboard">
            <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8'>
                <Card>
                    <CardHeader className='pb-3'>
                        <CardDescription>Wszystkie zgłoszenia</CardDescription>
                        <CardTitle className='text-3xl'>{mockStats.totalTickets}</CardTitle>
                    </CardHeader>
                </Card>
                <Card>
                    <CardHeader className='pb-3'>
                        <CardDescription>Otwarte</CardDescription>
                        <CardTitle className='text-3xl text-blue-600'>{mockStats.openTickets}</CardTitle>
                    </CardHeader>
                </Card>
                <Card>
                    <CardHeader className='pb-3'>
                        <CardDescription>W trakcie</CardDescription>
                        <CardTitle className='text-3xl text-orange-600'>{mockStats.inProgressTickets}</CardTitle>
                    </CardHeader>
                </Card>
                <Card>
                    <CardHeader className='pb-3'>
                        <CardDescription>Rozwiązane</CardDescription>
                        <CardTitle className='text-3xl text-green-600'>{mockStats.resolvedTickets}</CardTitle>
                    </CardHeader>
                </Card>
            </div>

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
                    <div className='space-y-4'>
                        {mockTickets.slice(0, 5).map((ticket) => (
                            <Card key={ticket.id} className='hover:shadow-lg transition-shadow cursor-pointer'>
                                <CardHeader>
                                    <div className="flex justify-between items-start">
                                        <div className='flex-1'>
                                            <div className='flex items-center gap-3 mb-2'>
                                                <CardTitle className='text-lg'>{ticket.title}</CardTitle>
                                                <Badge variant={getStatusVariant(ticket.status)}>
                                                    {ticket.status === 'Open' ? 'Otwarte' :
                                                        ticket.status === 'InProgress' ? 'W trakcie' : 'Rozwiązane'}
                                                </Badge>
                                                <span className={`text-sm font-semibold ${getPriorityColor(ticket.priority)}`}>
                                                    {ticket.priority === 'Critical' ? 'Krytyczny' :
                                                        ticket.priority === 'High' ? 'Wysoki' :
                                                            ticket.priority === 'Medium' ? 'Średni' : 'Niski'}
                                                </span>
                                            </div>
                                            <CardDescription className='line-clamp-2'>{ticket.description}</CardDescription>
                                        </div>
                                    </div>
                                </CardHeader>
                                <CardContent>
                                    <div className='flex justify-between items-center text-sm text-gray-600'>
                                        <div>
                                            {ticket.agentName ? (
                                                <span>Przypisany do: <span className='font-semibold'>{ticket.agentName}</span></span>
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
                        {mockTickets.length === 0 && (
                            <Card className='text-center py-12'>
                                <CardContent>
                                    <p className='text-gray-500 mb-4'>Brak zgłoszeń do wyświetlenia</p>
                                    <Button>Utwórz nowe zgłoszenie</Button>
                                </CardContent>
                            </Card>
                        )}
                    </div>
                </CardContent>
            </Card>
        </Layout>
    );
}