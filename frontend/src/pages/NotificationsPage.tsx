import { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle} from '@/components/ui/card';
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import Layout from "@/components/ui/Layout";
import { Bell, Check, Trash2, Mail, Ticket, UserPlus, AlertCircle} from 'lucide-react';

interface Notification{
    id: string;
    type: 'ticket' | 'user' | 'system' | 'alert';
    title: string;
    message: string;
    isRead: boolean;
    createdAt: string;
    link?: string;
}

export default function NotificationsPage() {
    const [notifications, setNotifications] = useState<Notification[]>([
        {
            id: '1',
            type: 'ticket',
            title: 'Nowe zgłoszenie przypisane',
            message: 'Zostałeś przypisany do zgłoszenia #1234 - Problem z logowaniem',
            isRead: false,
            createdAt: new Date(Date.now() - 5 * 60000).toISOString(),
            link: '/tickets/1234',
        },
        {
            id: '2',
            type: 'alert',
            title: 'Krytyczne zgłoszenie',
            message: 'Nowe zgłoszenie z priorytetem krytycznym wymaga natychmiastowej uwagi',
            isRead: false,
            createdAt: new Date(Date.now() - 15 * 60000).toISOString(),
            link: '/tickets/1235',
        },
        {
            id: '3',
            type: 'ticket',
            title: 'Zgłoszenie zaktualizowane',
            message: 'Klient dodał komentarz do zgłoszenia #1233',
            isRead: false,
            createdAt: new Date(Date.now() - 30 * 60000).toISOString(),
            link: '/tickets/1233',
        },
        {
            id: '4',
            type: 'user',
            title: 'Nowy użytkownik w systemie',
            message: 'Jan Kowalski zarejestrował się w systemie',
            isRead: true,
            createdAt: new Date(Date.now() - 3600000).toISOString(),
        },
        {
            id: '5',
            type: 'system',
            title: 'Aktualizacja systemu',
            message: 'System zostanie zaktualizowany 20 listopada o 02:00',
            isRead: true,
            createdAt: new Date(Date.now() - 7200000).toISOString(),
        },
        {
            id: '6',
            type: 'ticket',
            title: 'Zgłoszenie rozwiązane',
            message: 'Zgłoszenie #1232 zostało oznaczone jako rozwiązane',
            isRead: true,
            createdAt: new Date(Date.now() - 86400000).toISOString(),
            link: '/tickets/1232',
        },
    ]);

    const unreadCount = notifications.filter( n => !n.isRead).length;

    const markAsRead = (id:string) => {
        setNotifications(notifications.map( n =>
            n.id === id ? {...n,isRead: true} : n
        ));
    };

    const markAllAsRead = () => {
        setNotifications(notifications.map( n => ({...n, isRead: true})));
    }

    const deleteNotification = (id:string) => {
        setNotifications(notifications.filter( n => n.id !== id));
    }

    const getIcon = (type: string) => {
        switch (type) {
            case 'ticket':
                return <Ticket className="h-5 w-5 text-blue-600" />;
            case 'user':
                return <UserPlus className="h-5 w-5 text-green-600" />;
            case 'alert':
                return <AlertCircle className="h-5 w-5 text-red-600" />;
            case 'system':
                return <Bell className="h-5 w-5 text-gray-600" />;
            default:
                return <Mail className="h-5 w-5 text-gray-600" />;
        }
    };

    const getTimeAgo = (dateString: string) =>{
        const date = new Date(dateString);
        const now = new Date();
        const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

        if(seconds < 60) return 'Przed chwilą';
        if(seconds < 300) return `${Math.floor(seconds/60)} min. temu`;
        if(seconds < 86400) return `${Math.floor(seconds/3600)}  godz. temu`;
        if(seconds < 604800) return `${Math.floor(seconds/86400)} dni temu`;
        return date.toLocaleDateString('pl-PL');
    }

   return (
        <Layout currentPage="/notifications">
            <div className="space-y-6">
                <div className="flex justify-between items-center">
                    <div>
                        <h1 className="text-3xl font-bold text-slate-900">Powiadomienia</h1>
                        <p className="text-gray-600 mt-1">
                            {unreadCount > 0 
                                ? `Masz ${unreadCount} ${unreadCount === 1 ? 'nieprzeczytane powiadomienie' : 'nieprzeczytanych powiadomień'}`
                                : 'Wszystkie powiadomienia przeczytane'
                            }
                        </p>
                    </div>
                    {unreadCount > 0 && (
                        <Button variant="outline" onClick={markAllAsRead}>
                            <Check className="mr-2 h-4 w-4" />
                            Oznacz wszystkie jako przeczytane
                        </Button>
                    )}
                </div>

                <Card>
                    <CardHeader>
                        <div className="flex justify-between items-center">
                            <div>
                                <CardTitle>Wszystkie powiadomienia</CardTitle>
                                <CardDescription>
                                    {notifications.length} powiadomień w sumie
                                </CardDescription>
                            </div>
                            <Badge variant={unreadCount > 0 ? "default" : "outline"}>
                                {unreadCount} nowych
                            </Badge>
                        </div>
                    </CardHeader>
                    <CardContent>
                        {notifications.length === 0 ? (
                            <div className="text-center py-12">
                                <Bell className="mx-auto h-12 w-12 text-gray-400 mb-4" />
                                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                                    Brak powiadomień
                                </h3>
                                <p className="text-gray-500">
                                    Nie masz żadnych powiadomień do wyświetlenia
                                </p>
                            </div>
                        ) : (
                            <div className="space-y-2">
                                {notifications.map((notification) => (
                                    <div
                                        key={notification.id}
                                        className={`flex items-start gap-4 p-4 rounded-lg border transition-colors ${
                                            notification.isRead 
                                                ? 'bg-white border-gray-200' 
                                                : 'bg-blue-50 border-blue-200'
                                        } hover:shadow-md`}
                                    >
                                        <div className="flex-shrink-0 mt-1">
                                            {getIcon(notification.type)}
                                        </div>

                                        <div className="flex-1 min-w-0">
                                            <div className="flex items-start justify-between gap-2">
                                                <h3 className={`text-sm font-semibold ${
                                                    notification.isRead ? 'text-gray-700' : 'text-gray-900'
                                                }`}>
                                                    {notification.title}
                                                </h3>
                                                {!notification.isRead && (
                                                    <div className="w-2 h-2 bg-blue-600 rounded-full flex-shrink-0 mt-1"></div>
                                                )}
                                            </div>
                                            <p className="text-sm text-gray-600 mt-1">
                                                {notification.message}
                                            </p>
                                            <div className="flex items-center gap-4 mt-2">
                                                <span className="text-xs text-gray-500">
                                                    {getTimeAgo(notification.createdAt)}
                                                </span>
                                                {notification.link && (
                                                    <a 
                                                        href={notification.link}
                                                        className="text-xs text-blue-600 hover:underline"
                                                    >
                                                        Zobacz szczegóły →
                                                    </a>
                                                )}
                                            </div>
                                        </div>

                                        <div className="flex items-center gap-2 flex-shrink-0">
                                            {!notification.isRead && (
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    onClick={() => markAsRead(notification.id)}
                                                    title="Oznacz jako przeczytane"
                                                >
                                                    <Check className="h-4 w-4" />
                                                </Button>
                                            )}
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                onClick={() => deleteNotification(notification.id)}
                                                title="Usuń powiadomienie"
                                            >
                                                <Trash2 className="h-4 w-4 text-red-600" />
                                            </Button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </CardContent>
                </Card>
            </div>
        </Layout>
    );
}

