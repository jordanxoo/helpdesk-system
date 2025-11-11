import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ticketService } from '../services/ticketService';
import { mockTickets } from '../data/mockData';
import type { TicketDetails, TicketComment } from '../types/ticket.types';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Textarea } from '../components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { ArrowLeft, Send, Clock, User, AlertCircle } from 'lucide-react';

export default function TicketDetailsPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [ticket, setTicket] = useState<TicketDetails | null>(null);
    const [loading, setLoading] = useState(true);
    const [newComment, setNewComment] = useState('');
    const [submittingComment, setSubmittingComment] = useState(false);
    const [updatingStatus, setUpdatingStatus] = useState(false);

    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const isAgent = user.role === 'Agent' || user.role === 'Administrator';
    const isAdmin = user.role === 'Administrator';

    useEffect(() => {
        loadTicket();
    }, [id]);

    const loadTicket = async () => {
        if (!id) return;
        try {
            setLoading(true);
            const mockTicket = mockTickets.find(t => t.id === id);
            if (mockTicket) {
                setTicket({
                    ...mockTicket,
                    customerEmail: 'mock@example.com',
                    comments: [] 
                } as TicketDetails);
            }
            // TODO: Replace with real API call when backend has data
            // const data = await ticketService.getTicketById(id);
            // setTicket(data);
        } catch (error) {
            console.error('Failed to load ticket:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleAddComment = async () => {
        if (!id || !newComment.trim()) return;
        
        try {
            setSubmittingComment(true);
            await ticketService.addComment(id, newComment);
            setNewComment('');
            await loadTicket(); 
        } catch (error) {
            console.error('Failed to add comment:', error);
        } finally {
            setSubmittingComment(false);
        }
    };

    const handleStatusChange = async (newStatus: string) => {
        if (!id) return;
        
        try {
            setUpdatingStatus(true);
            await ticketService.updateTicketStatus(id, newStatus);
            await loadTicket();
        } catch (error) {
            console.error('Failed to update status:', error);
        } finally {
            setUpdatingStatus(false);
        }
    };

    const getPriorityColor = (priority: string) => {
        switch (priority) {
            case 'Critical': return 'bg-red-500';
            case 'High': return 'bg-orange-500';
            case 'Medium': return 'bg-yellow-500';
            case 'Low': return 'bg-green-500';
            default: return 'bg-gray-500';
        }
    };

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'Open': return 'bg-blue-500';
            case 'InProgress': return 'bg-purple-500';
            case 'Resolved': return 'bg-green-500';
            case 'Closed': return 'bg-gray-500';
            default: return 'bg-gray-500';
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Ładowanie zgłoszenia...</p>
                </div>
            </div>
        );
    }

    if (!ticket) {
        return (
            <div className="container mx-auto p-6">
                <div className="text-center">
                    <AlertCircle className="mx-auto h-12 w-12 text-red-500" />
                    <h2 className="mt-4 text-xl font-semibold">Nie znaleziono zgłoszenia</h2>
                    <Button onClick={() => navigate('/tickets')} className="mt-4">
                        <ArrowLeft className="mr-2 h-4 w-4" />
                        Powrót do listy
                    </Button>
                </div>
            </div>
        );
    }

    return (
        <div className="container mx-auto p-6 max-w-5xl">
            
            <div className="mb-6">
                <Button variant="ghost" onClick={() => navigate('/tickets')} className="mb-4">
                    <ArrowLeft className="mr-2 h-4 w-4" />
                    Powrót do listy
                </Button>
                
                <div className="flex items-start justify-between">
                    <div>
                        <h1 className="text-3xl font-bold mb-2">{ticket.title}</h1>
                        <div className="flex gap-2 items-center text-sm text-gray-600">
                            <span className="flex items-center">
                                <Clock className="mr-1 h-4 w-4" />
                                {new Date(ticket.createdAt).toLocaleDateString('pl-PL')}
                            </span>
                            <span className="flex items-center">
                                <User className="mr-1 h-4 w-4" />
                                {ticket.customerName}
                            </span>
                        </div>
                    </div>
                    <div className="flex gap-2">
                        <Badge className={getPriorityColor(ticket.priority)}>
                            {ticket.priority}
                        </Badge>
                        <Badge className={getStatusColor(ticket.status)}>
                            {ticket.status}
                        </Badge>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                
                <div className="lg:col-span-2 space-y-6">
                    
                    <Card>
                        <CardHeader>
                            <CardTitle>Opis problemu</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="whitespace-pre-wrap text-gray-700">{ticket.description}</p>
                        </CardContent>
                    </Card>

                    
                    <Card>
                        <CardHeader>
                            <CardTitle>Komentarze ({ticket.comments?.length || 0})</CardTitle>
                            <CardDescription>Historia komunikacji</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            
                            <div className="space-y-4 max-h-96 overflow-y-auto">
                                {ticket.comments && ticket.comments.length > 0 ? (
                                    ticket.comments.map((comment: TicketComment) => (
                                        <div key={comment.id} className="border-l-4 border-blue-500 pl-4 py-2">
                                            <div className="flex items-center justify-between mb-1">
                                                <div className="flex items-center gap-2">
                                                    <span className="font-semibold text-sm">{comment.userName}</span>
                                                    <Badge variant="outline" className="text-xs">
                                                        {comment.userRole}
                                                    </Badge>
                                                </div>
                                                <span className="text-xs text-gray-500">
                                                    {new Date(comment.createdAt).toLocaleString('pl-PL')}
                                                </span>
                                            </div>
                                            <p className="text-gray-700 text-sm">{comment.content}</p>
                                        </div>
                                    ))
                                ) : (
                                    <p className="text-center text-gray-500 py-8">Brak komentarzy</p>
                                )}
                            </div>

                            <div className="pt-4 border-t">
                                <Textarea
                                    placeholder="Dodaj komentarz..."
                                    value={newComment}
                                    onChange={(e) => setNewComment(e.target.value)}
                                    className="mb-2"
                                    rows={3}
                                />
                                <Button 
                                    onClick={handleAddComment}
                                    disabled={!newComment.trim() || submittingComment}
                                    className="w-full"
                                >
                                    <Send className="mr-2 h-4 w-4" />
                                    {submittingComment ? 'Wysyłanie...' : 'Dodaj komentarz'}
                                </Button>
                            </div>
                        </CardContent>
                    </Card>
                </div>

                <div className="space-y-6">
                    {isAgent && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Zarządzanie</CardTitle>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                <div>
                                    <label className="text-sm font-medium mb-2 block">Status</label>
                                    <Select 
                                        value={ticket.status} 
                                        onValueChange={handleStatusChange}
                                        disabled={updatingStatus}
                                    >
                                        <SelectTrigger>
                                            <SelectValue />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Open">Open</SelectItem>
                                            <SelectItem value="InProgress">In Progress</SelectItem>
                                            <SelectItem value="Resolved">Resolved</SelectItem>
                                            <SelectItem value="Closed">Closed</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>

                                {isAdmin && (
                                    <Button 
                                        variant="destructive" 
                                        className="w-full"
                                        onClick={async () => {
                                            if (confirm('Czy na pewno chcesz usunąć to zgłoszenie?')) {
                                                await ticketService.deleteTicket(ticket.id);
                                                navigate('/tickets');
                                            }
                                        }}
                                    >
                                        Usuń zgłoszenie
                                    </Button>
                                )}
                            </CardContent>
                        </Card>
                    )}

\                    <Card>
                        <CardHeader>
                            <CardTitle>Szczegóły</CardTitle>
                        </CardHeader>
                        <CardContent className="space-y-3 text-sm">
                            <div>
                                <span className="font-semibold">ID:</span>
                                <p className="text-gray-600">{ticket.id}</p>
                            </div>
                            <div>
                                <span className="font-semibold">Kategoria:</span>
                                <p className="text-gray-600">{ticket.category}</p>
                            </div>
                            <div>
                                <span className="font-semibold">Przypisany agent:</span>
                                <p className="text-gray-600">
                                    {ticket.assignedAgentName || 'Nieprzypisany'}
                                </p>
                            </div>
                            <div>
                                <span className="font-semibold">Utworzono:</span>
                                <p className="text-gray-600">
                                    {new Date(ticket.createdAt).toLocaleString('pl-PL')}
                                </p>
                            </div>
                            <div>
                                <span className="font-semibold">Zaktualizowano:</span>
                                <p className="text-gray-600">
                                    {new Date(ticket.updatedAt).toLocaleString('pl-PL')}
                                </p>
                            </div>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
}