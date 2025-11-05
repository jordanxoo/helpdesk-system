import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { mockStats, mockTickets, mockUser } from '@/data/mockData';
import { Link } from 'react-router-dom';
import { Ticket } from 'lucide-react';

export default function DashboardPage()
{
    const getStatusVariant = (status: string) => {
        switch (status)
        {
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

    const getPriorityColor = (priority: string) =>
    {
        switch (priority)
        {
        case 'Critical':
            return 'text-red-600';

        case 'High':
            return 'text-oragne-600';
        case 'Medium':
            return 'text-yellow-600';
        case 'Low':
            return 'text-green-600';
        default:
            return 'text-grey-600';

        }
    };
    return(
        <div className='min-h-screen bg-gradient-to-br from-slate-50 to slate-100'>
            {/*NAVBAR*/}
            <nav className='bg-white border-b border-gray-200 shadow-sm'>
                <div className='max-w-7xl mx-auto px-4 sm:px-6 lg:px-8'>
                    <div className='flex justify-between items-center h-16'>
                        <div className='flex items-center'>
                        <h1 className='text-2xl font-bold text-slate-900'>HelpdeskSystem</h1>
                        </div>
                        <div className='flex items-center space-x-4'>
                            <span className='text-sm text-gray-700'>
                                Witaj, <span className='font-semibold'>{mockUser.fullName}</span>
                            </span>
                            <Button variant = "outline" size = 'sm'>Wyloguj</Button>
                        </div>
                    </div>
                </div>
            </nav>
            {/*MainContent*/}
            <main className='max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8'>
                <div className='grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-9'>
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
                        <CardTitle className='text-3xl text-green-600'>{mockStats.inProgressTickets}</CardTitle>
                    </CardHeader>
                    </Card>
                </div>
                <div className='flex justify-between items-center mb-6'>
                    <h2 className='text-2xl font-bold text-slate-900'>Ostatnie zgłoszenia</h2>
                    <Button>
                        <Link to = "/tickets">Zobacz wszystkie</Link>
                    </Button>
                </div>

                <div className='grid grid-cols-1 gap-4'>
                    {mockTickets.map((ticket) =>(
                        <Card key={ticket.id} className='hover:shadow-lg transition-shadow cursor-pointer'>
                            <CardHeader>
                                <div className="flex justify-between items-start">
                                    <div className='flex-1'>
                                        <div className='flex items-centre gap-3 mb-2'>
                                            <CardTitle className='text-lg'>{ticket.title}</CardTitle>
                                            <Badge variant={getStatusVariant(ticket.status)}>
                                                {ticket.status == 'Open' ? 'Otwarte' :
                                                ticket.status == 'InProgress' ? 'W trakcie' : 'Rozwiazane'}
                                            </Badge>

                                            <span className={`text-sm font semibold ${getPriorityColor(ticket.priority)}`}>
                                                {ticket.priority == 'Critical' ? 'Krytyczny' :
                                                ticket.priority == 'High' ? 'Wysoki' :
                                                ticket.priority == 'Medium' ? 'Średni' :
                                                'Niski'
                                                }
                                            </span>
                                        </div>
                                        <CardDescription className='line-clamp-2'>{ticket.description}</CardDescription>
                                    </div>
                                </div>
                            </CardHeader>
                            <CardContent>
                                <div className='flex justify-between items-center text-sm text-grey-600'>
                                    <div>
                                        {ticket.agentName ? (
                                            <span>Przypisany do: <span className='font-semibold'>{ticket.agentName}</span> </span>
                                        ): (
                                            <span className='text-grey-400'>Nieprzypisany</span>
                                        )}
                                    </div>
                                <div>
                                    {new Date(ticket.createdAt).toLocaleDateString('pl-PL',{
                                        day: 'numeric',
                                        month: 'long',
                                        year: 'numeric',
                                    })}
                                </div>
                                {mockTickets.length == 0 && (
                                    <Card className='=="text-center py-12'>
                                        <CardContent>
                                            <p className='text-gray-500 mb-4'>Brak zgłoszeń do wyświetlenia</p>
                                            <Button>Utwórz nowe zgłoszenie</Button>
                                        </CardContent>
                                    </Card>
                                )}
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                    
                </div>
            </main>
        </div>
    )
}