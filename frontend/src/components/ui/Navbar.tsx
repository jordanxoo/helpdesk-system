import { useNavigate } from "react-router-dom";
import { Button } from "./button";
import { Menu, X, User, LogOut } from "lucide-react";
import { useState } from "react";

interface NavbarProps{
    currentPage?: string;
}

export default function Navbar( {currentPage}: NavbarProps)
{
    const navigate = useNavigate();
    const [mobileMenuOpen,setMobileMenuOpen] = useState(false);

    const user = JSON.parse(localStorage.getItem('user') || '{}');
    const isAdmin = user.role === 'Administrator';

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        navigate('/login');
    };

    const navItems = [
        {label: 'Dashboard', path: '/dashboard', show: true},
        {label: 'Zgłoszenia', path: '/tickets', show: true},
        {label: 'Użytkownicy', path: '/users', show: isAdmin},
        {label: 'Panel Admin', path: '/admin', show: isAdmin},
        {label: 'Powiadomienia', path: '/notifications', show: true},
        {label: 'Profil', path: '/profile', show: true},
    ];

    return (
        <nav className="bg-white border-b border-gray-200 shadow-sm">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between items-center h-16">
                    <div className="flex items-center">
                        <button
                            onClick={() => navigate('/dashboard')}
                            className="text-2xl font-bold text-slate-900 hover:text-blue-600 transition-colors"
                        >
                            HelpdeskSystem
                        </button>
                    </div>

                    <div className="hidden md:flex items-center space-x-4">
                        {navItems.map((item) => 
                            item.show && (
                                <Button
                                    key={item.path}
                                    variant={currentPage === item.path ? 'default' : 'ghost'}
                                    onClick={() => navigate(item.path)}
                                >
                                    {item.label}
                                </Button>
                            )
                        )}
                    </div>
                     <div className="hidden md:flex items-center space-x-4">
                        <div className="flex items-center gap-2 text-sm text-gray-700">
                            <User className="h-4 w-4" />
                            <span>{user.fullName || user.email}</span>
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

                {mobileMenuOpen && (
                    <div className="md:hidden pb-4 space-y-2">
                        {navItems.map((item) =>
                            item.show && (
                                <Button
                                    key={item.path}
                                    variant={currentPage === item.path ? 'default' : 'ghost'}
                                    onClick={() => {
                                        navigate(item.path);
                                        setMobileMenuOpen(false);
                                    }}
                                    className="w-full justify-start"
                                >
                                    {item.label}
                                </Button>
                            )
                        )}
                        <div className="pt-2 border-t">
                            <div className="px-4 py-2 text-sm text-gray-700">
                                {user.fullName || user.email}
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