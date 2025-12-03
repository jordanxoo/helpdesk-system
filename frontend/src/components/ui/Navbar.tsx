import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Menu, X, User, LogOut, Shield, Users as UsersIcon, Ticket } from 'lucide-react';
import { useState } from 'react';
import { Badge } from '@/components/ui/badge';

interface NavbarProps {
    currentPage?: string;
}

export default function Navbar({ currentPage }: NavbarProps) {
    const navigate = useNavigate();
    const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

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
    const role = user?.role as 'Customer' | 'Agent' | 'Administrator' | undefined;

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        navigate('/login');
        // Force reload to clear any cached state
        window.location.reload();
    };

    const getRoleBadgeColor = () => {
        switch (role) {
            case 'Administrator':
                return 'bg-red-500 text-white';
            case 'Agent':
                return 'bg-purple-500 text-white';
            case 'Customer':
                return 'bg-blue-500 text-white';
            default:
                return 'bg-gray-500 text-white';
        }
    };

    const getRoleLabel = () => {
        switch (role) {
            case 'Administrator':
                return 'Administrator';
            case 'Agent':
                return 'Agent';
            case 'Customer':
                return 'Klient';
            default:
                return role || 'Użytkownik';
        }
    };

    // Różne menu items dla różnych ról
    const getNavItems = () => {
        if (!role) return [];
        
        switch (role) {
            case 'Administrator':
                return [
                    { label: 'Panel Admin', path: '/admin', icon: Shield, show: true },
                    { label: 'Użytkownicy', path: '/users', icon: UsersIcon, show: true },
                    { label: 'Wszystkie zgłoszenia', path: '/tickets', icon: Ticket, show: true },
                    { label: 'Profil', path: '/profile', icon: User, show: true },
                ];
            case 'Agent':
                return [
                    { label: 'Dashboard', path: '/dashboard', icon: Ticket, show: true },
                    { label: 'Moje zgłoszenia', path: '/tickets', icon: Ticket, show: true },
                    { label: 'Profil', path: '/profile', icon: User, show: true },
                ];
            case 'Customer':
                return [
                    { label: 'Dashboard', path: '/dashboard', icon: Ticket, show: true },
                    { label: 'Moje zgłoszenia', path: '/tickets', icon: Ticket, show: true },
                    { label: 'Nowe zgłoszenie', path: '/tickets/create', icon: Ticket, show: true },
                    { label: 'Profil', path: '/profile', icon: User, show: true },
                ];
            default:
                return [];
        }
    };

    const navItems = getNavItems();

    return (
        <nav className="bg-white border-b border-gray-200 shadow-sm">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between items-center h-16">
                    {/* Logo */}
                    <div className="flex items-center space-x-4">
                        <button
                            onClick={() => {
                                // Różne przekierowania dla różnych ról
                                if (role === 'Administrator') navigate('/admin');
                                else navigate('/dashboard');
                            }}
                            className="text-2xl font-bold text-slate-900 hover:text-blue-600 transition-colors"
                        >
                            HelpdeskSystem
                        </button>
                        <Badge className={getRoleBadgeColor()}>
                            {getRoleLabel()}
                        </Badge>
                    </div>

                    {/* Desktop Navigation */}
                    <div className="hidden md:flex items-center space-x-2">
                        {navItems.map((item) => {
                            const Icon = item.icon;
                            return item.show && (
                                <Button
                                    key={item.path}
                                    variant={currentPage === item.path ? 'default' : 'ghost'}
                                    onClick={() => navigate(item.path)}
                                    className="flex items-center gap-2"
                                >
                                    <Icon className="h-4 w-4" />
                                    {item.label}
                                </Button>
                            );
                        })}
                    </div>

                    {/* User Menu */}
                    <div className="hidden md:flex items-center space-x-4">
                        <div className="flex items-center gap-2 text-sm text-gray-700">
                            <User className="h-4 w-4" />
                            <span>{user?.fullName || user?.email}</span>
                        </div>
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={handleLogout}
                        >
                            <LogOut className="h-4 w-4 mr-2" />
                            Wyloguj
                        </Button>
                    </div>

                    {/* Mobile menu button */}
                    <div className="md:hidden">
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
                        >
                            {mobileMenuOpen ? (
                                <X className="h-6 w-6" />
                            ) : (
                                <Menu className="h-6 w-6" />
                            )}
                        </Button>
                    </div>
                </div>

                {/* Mobile Navigation */}
                {mobileMenuOpen && (
                    <div className="md:hidden pb-4 space-y-2">
                        {navItems.map((item) => {
                            const Icon = item.icon;
                            return item.show && (
                                <Button
                                    key={item.path}
                                    variant={currentPage === item.path ? 'default' : 'ghost'}
                                    onClick={() => {
                                        navigate(item.path);
                                        setMobileMenuOpen(false);
                                    }}
                                    className="w-full justify-start flex items-center gap-2"
                                >
                                    <Icon className="h-4 w-4" />
                                    {item.label}
                                </Button>
                            );
                        })}
                        <div className="pt-2 border-t">
                            <div className="px-4 py-2 text-sm text-gray-700 flex items-center gap-2">
                                <Badge className={getRoleBadgeColor()}>
                                    {getRoleLabel()}
                                </Badge>
                                {user?.fullName || user?.email}
                            </div>
                            <Button
                                variant="outline"
                                onClick={handleLogout}
                                className="w-full justify-start"
                            >
                                <LogOut className="h-4 w-4 mr-2" />
                                Wyloguj
                            </Button>
                        </div>
                    </div>
                )}
            </div>
        </nav>
    );
}