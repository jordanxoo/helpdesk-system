import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ticketService } from '../services/ticketService';
//import { mockTickets } from '../data/mockData';
import type { TicketDetails, TicketComment, TicketAttachment } from '../types/ticket.types';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Textarea } from '../components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { ArrowLeft, Send, Clock, User, AlertCircle, Paperclip, Upload, FileText, Image, File, Download, X, ZoomIn } from 'lucide-react';
import { userService } from '../services/userService';
import { validateFile, FILE_UPLOAD_LIMITS } from '../constants/fileValidation';

export default function TicketDetailsPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [ticket, setTicket] = useState<TicketDetails | null>(null);
    const [loading, setLoading] = useState(true);
    const [newComment, setNewComment] = useState('');
    const [submittingComment, setSubmittingComment] = useState(false);
    const [updatingStatus, setUpdatingStatus] = useState(false);
    const [agents, setAgents] = useState<any[]>([]);
    const [uploadingFile, setUploadingFile] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [previewImage, setPreviewImage] = useState<TicketAttachment | null>(null);
    const [imageLoadErrors, setImageLoadErrors] = useState<Set<string>>(new Set());

    // Bezpieczne pobieranie użytkownika z localStorage
    const getUserFromStorage = () => {
        try {
            const userStr = localStorage.getItem('user');
            if (!userStr || userStr === 'undefined') {
                return null;
            }
            return JSON.parse(userStr);
        } catch {
            return null;
        }
    };

    const user = getUserFromStorage();
    const isCustomer = user?.role === 'Customer';
    const isAgent = user?.role === 'Agent' || user?.role === 'Administrator';
    const isAdmin = user?.role === 'Administrator';
    const isAssignedToMe = ticket?.assignedAgentId === user?.id;

    // Handle escape key for modal
    useEffect(() => {
        const handleEscape = (e: KeyboardEvent) => {
            if (e.key === 'Escape' && previewImage) {
                setPreviewImage(null);
            }
        };
        window.addEventListener('keydown', handleEscape);
        return () => window.removeEventListener('keydown', handleEscape);
    }, [previewImage]);

    useEffect(() => {
        const currentUser = getUserFromStorage();
        
        // Sprawdź czy użytkownik jest zalogowany
        if (!currentUser) {
            navigate('/login', { replace: true });
            return;
        }

        const fetchData = async () => {
            if (!id) return;
            
            try {
                setLoading(true);
                const data = await ticketService.getTicketById(id);
                setTicket(data);
                
                // Załaduj agentów tylko dla administratora
                if (currentUser?.role === 'Administrator') {
                    try {
                        const response = await userService.getAllUsers();
                        const agentList = response.filter((u: any) => u.role === 'Agent');
                        setAgents(agentList);
                    } catch (error) {
                        console.error('Failed to load agents:', error);
                    }
                }
            } catch (error) {
                console.error('Failed to load ticket:', error);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id, navigate]);

    const handleAddComment = async () => {
        if (!id || !newComment.trim()) return;
        
        try {
            setSubmittingComment(true);
            const updatedTicket = await ticketService.addComment(id, newComment);
            setNewComment('');
            setTicket(updatedTicket);
        } catch (error: any) {
            console.error('Failed to add comment:', error);
            const message = error.response?.data?.message || 'Nie udało się dodać komentarza';
            alert(message);
        } finally {
            setSubmittingComment(false);
        }
    };

    const handleStatusChange = async (newStatus: string) => {
        if (!id) return;
        
        try {
            setUpdatingStatus(true);
            const updatedTicket = await ticketService.updateTicketStatus(id, newStatus);
            setTicket(updatedTicket);
        } catch (error: any) {
            console.error('Failed to update status:', error);
            const message = error.response?.data?.message || 'Nie udało się zmienić statusu';
            alert(message);
        } finally {
            setUpdatingStatus(false);
        }
    };

    const handleAssignToMe = async () => {
        if (!id || !user?.id) return;
        
        try {
            const updatedTicket = await ticketService.assignTicket(id, user.id);
            setTicket(updatedTicket);
        } catch (error: any) {
            console.error("Failed to assign ticket:", error);
            const message = error.response?.data?.message || 'Nie udało się przypisać zgłoszenia';
            alert(message);
        }
    }

    const handleUnassign = async () => {
        if (!id) return;
        
        try {
            const updatedTicket = await ticketService.assignTicket(id, ""); // pusty string = unassign
            setTicket(updatedTicket);
        } catch (error: any) {
            console.error("Failed to unassign ticket:", error);
            const message = error.response?.data?.message || 'Nie udało się usunąć przypisania';
            alert(message);
        }
    };

    const handlePriorityChange = async (newPriority: string) => {
        if (!id) return;
        
        try {
            const updatedTicket = await ticketService.updateTicketPriority(id, newPriority);
            setTicket(updatedTicket);
        } catch (error: any) {
            console.error("Failed to change priority:", error);
            const message = error.response?.data?.message || 'Nie udało się zmienić priorytetu';
            alert(message);
        }
    };

    const handleAssignAgent = async (agentID: string) => {
        if (!id) return;
        
        try {
            const updatedTicket = await ticketService.assignTicket(id, agentID);
            setTicket(updatedTicket);
        } catch (error: any) {
            console.error("Failed to assign agent:", error);
            const message = error.response?.data?.message || 'Nie udało się przypisać agenta';
            alert(message);
        }
    };

    const handleCloseTicket = async () => {
        if (!id) return;
        
        if (confirm('Czy na pewno chcesz zamknąć to zgłoszenie?')) {
            try {
                const updatedTicket = await ticketService.closeTicket(id);
                setTicket(updatedTicket);
            } catch (error: any) {
                console.error('Failed to close ticket:', error);
                // Show error message to user
                const message = error.response?.data?.message || 'Nie udało się zamknąć zgłoszenia';
                alert(message);
            }
        }
    };

    const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file || !id) return;

        // Validate file
        const validation = validateFile(file);
        if (!validation.isValid) {
            alert(validation.error);
            return;
        }

        try {
            setUploadingFile(true);
            const newAttachment = await ticketService.uploadAttachment(id, file);
            
            // Dodaj nowy załącznik do listy
            setTicket(prev => {
                if (!prev) return prev;
                return {
                    ...prev,
                    attachment: [...(prev.attachment || []), newAttachment]
                };
            });
            
            // Reset input
            if (fileInputRef.current) {
                fileInputRef.current.value = '';
            }
        } catch (error: any) {
            console.error('Failed to upload attachment:', error);
            const message = error.response?.data?.message || 'Nie udało się przesłać pliku';
            alert(message);
        } finally {
            setUploadingFile(false);
        }
    };

    const formatFileSize = (bytes: number): string => {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    const getFileIcon = (contentType: string) => {
        if (contentType.startsWith('image/')) {
            return <Image className="h-5 w-5 text-blue-500" />;
        }
        if (contentType === 'application/pdf') {
            return <FileText className="h-5 w-5 text-red-500" />;
        }
        return <File className="h-5 w-5 text-gray-500" />;
    };

    const isImageFile = (contentType: string) => {
        return contentType.startsWith('image/');
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
                                    ticket.comments.map((comment: TicketComment) => {
                                        const borderColor = comment.userRole === 'Customer' ? '#3b82f6' 
                                            : comment.userRole === 'Agent' ? '#a855f7' 
                                            : comment.userRole === 'Administrator' ? '#ef4444' 
                                            : '#3b82f6';
                                        return (
                                            <div key={comment.id} className="border-l-4 pl-4 py-2" style={{ borderColor }}>
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
                                        );
                                    })
                                ) : (
                                    <p className="text-center text-gray-500 py-8">Brak komentarzy</p>
                                )}
                            </div>

                            {/* Formularz komentarza - różne uprawnienia dla różnych ról */}
                            <div className="pt-4 border-t">
                                {/* AGENT i ADMINISTRATOR - mogą komentować wszystkie zgłoszenia */}
                                {/* KLIENT - może komentować tylko swoje zgłoszenia */}
                                {(isAgent || (isCustomer && ticket.customerId === user?.id)) ? (
                                    <>
                                        <Textarea
                                            placeholder={
                                                isAgent 
                                                    ? "Dodaj komentarz do zgłoszenia..." 
                                                    : "Dodaj komentarz do swojego zgłoszenia..."
                                            }
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
                                    </>
                                ) : (
                                    <div className="p-4 bg-gray-50 rounded-lg text-center">
                                        <p className="text-sm text-gray-600">
                                            Nie możesz komentować tego zgłoszenia
                                        </p>
                                    </div>
                                )}
                            </div>
                        </CardContent>
                    </Card>

                    {/* Sekcja załączników */}
                    <Card>
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <Paperclip className="h-5 w-5" />
                                Załączniki ({ticket.attachment?.length || 0})
                            </CardTitle>
                            <CardDescription>Pliki dołączone do zgłoszenia</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            {ticket.attachment && ticket.attachment.length > 0 ? (
                                <>
                                    {/* Podgląd obrazków */}
                                    {ticket.attachment.filter(a => isImageFile(a.contentType)).length > 0 && (
                                        <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                                            {ticket.attachment
                                                .filter(a => isImageFile(a.contentType))
                                                .map((attachment: TicketAttachment) => (
                                                    <div 
                                                        key={attachment.id}
                                                        className="relative group cursor-pointer rounded-lg overflow-hidden border bg-gray-100 aspect-square"
                                                        onClick={() => setPreviewImage(attachment)}
                                                    >
                                                        {imageLoadErrors.has(attachment.id) ? (
                                                            <div className="w-full h-full flex items-center justify-center bg-gray-200">
                                                                <span className="text-gray-500 text-xs text-center px-2">
                                                                    {attachment.fileName}
                                                                </span>
                                                            </div>
                                                        ) : (
                                                            <>
                                                                <img 
                                                                    src={attachment.downloadUrl} 
                                                                    alt={attachment.fileName}
                                                                    className="w-full h-full object-cover transition-transform group-hover:scale-105"
                                                                    onError={() => {
                                                                        setImageLoadErrors(prev => new Set(prev).add(attachment.id));
                                                                    }}
                                                                />
                                                                <div className="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-colors flex items-center justify-center">
                                                                    <ZoomIn className="h-8 w-8 text-white opacity-0 group-hover:opacity-100 transition-opacity" />
                                                                </div>
                                                            </>
                                                        )}
                                                        <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/60 to-transparent p-2">
                                                            <p className="text-white text-xs truncate">{attachment.fileName}</p>
                                                        </div>
                                                    </div>
                                                ))}
                                        </div>
                                    )}

                                    {/* Lista pozostałych plików (nie-obrazki) */}
                                    {ticket.attachment.filter(a => !isImageFile(a.contentType)).length > 0 && (
                                        <div className="space-y-2">
                                            {ticket.attachment
                                                .filter(a => !isImageFile(a.contentType))
                                                .map((attachment: TicketAttachment) => (
                                                    <div 
                                                        key={attachment.id} 
                                                        className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border hover:bg-gray-100 transition-colors"
                                                    >
                                                        <div className="flex items-center gap-3">
                                                            {getFileIcon(attachment.contentType)}
                                                            <div>
                                                                <p className="font-medium text-sm text-gray-900">
                                                                    {attachment.fileName}
                                                                </p>
                                                                <p className="text-xs text-gray-500">
                                                                    {formatFileSize(attachment.fileSizeBytes)} • {new Date(attachment.uploadedAt).toLocaleString('pl-PL')}
                                                                </p>
                                                            </div>
                                                        </div>
                                                        <a 
                                                            href={attachment.downloadUrl} 
                                                            target="_blank" 
                                                            rel="noopener noreferrer"
                                                            className="flex items-center gap-1 text-blue-600 hover:text-blue-800 text-sm font-medium"
                                                        >
                                                            <Download className="h-4 w-4" />
                                                            Pobierz
                                                        </a>
                                                    </div>
                                                ))}
                                        </div>
                                    )}
                                </>
                            ) : (
                                <p className="text-center text-gray-500 py-4">Brak załączników</p>
                            )}

                            {/* Formularz dodawania załącznika - dostępny dla uprawnionych użytkowników */}
                            {(isAgent || (isCustomer && ticket.customerId === user?.id)) && (
                                <div className="pt-4 border-t">
                                    <input
                                        type="file"
                                        ref={fileInputRef}
                                        onChange={handleFileUpload}
                                        className="hidden"
                                        id="file-upload"
                                        accept={FILE_UPLOAD_LIMITS.ACCEPT_ATTRIBUTE}
                                    />
                                    <label htmlFor="file-upload">
                                        <Button 
                                            variant="outline" 
                                            className="w-full cursor-pointer"
                                            disabled={uploadingFile}
                                            asChild
                                        >
                                            <span>
                                                <Upload className="mr-2 h-4 w-4" />
                                                {uploadingFile ? 'Przesyłanie...' : 'Dodaj załącznik'}
                                            </span>
                                        </Button>
                                    </label>
                                    <p className="text-xs text-gray-500 mt-2 text-center">
                                        Maksymalny rozmiar pliku: 10 MB
                                    </p>
                                </div>
                            )}
                        </CardContent>
                    </Card>
                </div>

                <div className="space-y-6">
                    {/* PANEL DLA AGENTA I ADMINISTRATORA */}
                    {isAgent && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Zarządzanie zgłoszeniem</CardTitle>
                                <CardDescription>
                                    {isAdmin ? 'Panel administratora' : 'Panel agenta'}
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                
                                {/* TYLKO AGENT - Przycisk "Przypisz mnie" */}
                                {!isAdmin && !ticket.assignedAgentId && (
                                    <Button 
                                        onClick={handleAssignToMe}
                                        className="w-full"
                                        variant="default"
                                    >
                                        <User className="mr-2 h-4 w-4" />
                                        Przypisz mnie do zgłoszenia
                                    </Button>
                                )}

                                {/* TYLKO AGENT - Przycisk "Usuń przypisanie" (tylko dla przypisanego) */}
                                {!isAdmin && isAssignedToMe && (
                                    <Button 
                                        onClick={handleUnassign}
                                        className="w-full"
                                        variant="outline"
                                    >
                                        Usuń moje przypisanie
                                    </Button>
                                )}

                                {/* TYLKO ADMINISTRATOR - Dropdown przypisania agenta */}
                                {isAdmin && (
                                    <div>
                                        <label className="text-sm font-medium mb-2 block">
                                            Przypisz agenta
                                        </label>
                                        <Select 
                                            value={ticket.assignedAgentId || 'unassigned'} 
                                            onValueChange={(value) => handleAssignAgent(value === 'unassigned' ? '' : value)}
                                        >
                                            <SelectTrigger>
                                                <SelectValue placeholder="Wybierz agenta..." />
                                            </SelectTrigger>
                                            <SelectContent>
                                                <SelectItem value="unassigned">Nieprzypisany</SelectItem>
                                                {agents.map((agent) => (
                                                    <SelectItem key={agent.id} value={agent.id}>
                                                        {agent.fullName || agent.email}
                                                    </SelectItem>
                                                ))}
                                            </SelectContent>
                                        </Select>
                                    </div>
                                )}

                                {/* AGENT I ADMINISTRATOR - Zmiana statusu */}
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
                                            <SelectItem value="Open">Otwarte</SelectItem>
                                            <SelectItem value="InProgress">W trakcie</SelectItem>
                                            <SelectItem value="Resolved">Rozwiązane</SelectItem>
                                            <SelectItem value="Closed">Zamknięte</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>

                                {/* AGENT I ADMINISTRATOR - Zmiana priorytetu */}
                                <div>
                                    <label className="text-sm font-medium mb-2 block">Priorytet</label>
                                    <Select 
                                        value={ticket.priority} 
                                        onValueChange={handlePriorityChange}
                                    >
                                        <SelectTrigger>
                                            <SelectValue />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Low">Niski</SelectItem>
                                            <SelectItem value="Medium">Średni</SelectItem>
                                            <SelectItem value="High">Wysoki</SelectItem>
                                            <SelectItem value="Critical">Krytyczny</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>

                                {/* TYLKO ADMINISTRATOR - Usuń zgłoszenie */}
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

                    {/* PANEL DLA KLIENTA - tylko dla właściciela zgłoszenia */}
                    {isCustomer && ticket.customerId === user?.id && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Akcje</CardTitle>
                                <CardDescription>Zarządzaj swoim zgłoszeniem</CardDescription>
                            </CardHeader>
                            <CardContent className="space-y-4">
                    
                                {/* TYLKO KLIENT - Zamknij zgłoszenie (gdy status = Resolved) */}
                                {ticket.status === 'Resolved' && (
                                    <div className="space-y-2">
                                        <p className="text-sm text-gray-600">
                                            Twoje zgłoszenie zostało rozwiązane. Możesz je zamknąć.
                                        </p>
                                        <Button 
                                            onClick={handleCloseTicket}
                                            className="w-full"
                                            variant="default"
                                        >
                                            Zamknij zgłoszenie
                                        </Button>
                                    </div>
                                )}

                                {/* TYLKO KLIENT - Informacja o zamkniętym zgłoszeniu */}
                                {ticket.status === 'Closed' && (
                                    <div className="p-4 bg-gray-100 rounded-lg">
                                        <p className="text-sm text-gray-600 text-center">
                                            ✓ Zgłoszenie zamknięte
                                        </p>
                                    </div>
                                )}

                                {/* TYLKO KLIENT - Informacja o aktywnym zgłoszeniu */}
                                {ticket.status !== 'Resolved' && ticket.status !== 'Closed' && (
                                    <div className="p-4 bg-blue-50 rounded-lg">
                                        <p className="text-sm text-blue-800">
                                            💡 Twoje zgłoszenie jest aktywnie przetwarzane. 
                                            Możesz dodawać komentarze poniżej.
                                        </p>
                                    </div>
                                )}
                            </CardContent>
                        </Card>
                    )}

                    {/* PANEL DLA KLIENTA - Read-only dla cudzych zgłoszeń */}
                    {isCustomer && ticket.customerId !== user?.id && (
                        <Card>
                            <CardHeader>
                                <CardTitle>Informacja</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
                                    <p className="text-sm text-yellow-800">
                                        ⚠️ To nie jest Twoje zgłoszenie. Możesz tylko przeglądać.
                                    </p>
                                </div>
                            </CardContent>
                        </Card>
                    )}
                  <Card>
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

            {/* Modal podglądu obrazu */}
            {previewImage && (
                <div 
                    className="fixed inset-0 bg-black/90 z-50 flex flex-col items-center justify-center p-4"
                    onClick={() => setPreviewImage(null)}
                    role="dialog"
                    aria-modal="true"
                    aria-labelledby="preview-title"
                >
                    {/* Header z info o pliku */}
                    <div className="w-full max-w-4xl mb-4 flex items-center justify-between">
                        <div className="text-white">
                            <p id="preview-title" className="font-medium text-lg">{previewImage.fileName}</p>
                            <p className="text-gray-400 text-sm">
                                {formatFileSize(previewImage.fileSizeBytes)} • {new Date(previewImage.uploadedAt).toLocaleString('pl-PL')}
                            </p>
                        </div>
                        <button
                            onClick={() => setPreviewImage(null)}
                            className="text-white hover:text-gray-300 transition-colors p-2"
                            aria-label="Zamknij podgląd"
                        >
                            <X className="h-8 w-8" />
                        </button>
                    </div>
                    
                    {/* Obraz */}
                    <img 
                        src={previewImage.downloadUrl}
                        alt={previewImage.fileName}
                        className="max-w-4xl w-full h-auto max-h-[80vh] object-contain rounded-lg"
                        onClick={(e) => e.stopPropagation()}
                    />
                </div>
            )}
        </div>
    );
}