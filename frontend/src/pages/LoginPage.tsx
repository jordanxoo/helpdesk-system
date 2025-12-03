import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '@/services/authService';
import { userService } from '@/services/userService';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { LogIn, UserCircle, Shield, Users } from 'lucide-react';

export default function LoginPage() {
    const navigate = useNavigate();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [selectedRole, setSelectedRole] = useState<'Customer' | 'Agent' | 'Administrator'>('Customer');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    // Redirect if already logged in
    useEffect(() => {
        const token = localStorage.getItem('token');
        const userStr = localStorage.getItem('user');
        
        if (token && userStr) {
            try {
                const user = JSON.parse(userStr);
                // Redirect based on role
                if (user.role === 'Administrator') {
                    navigate('/admin', { replace: true });
                } else {
                    navigate('/dashboard', { replace: true });
                }
            } catch (error) {
                // Invalid user data, clear storage
                localStorage.removeItem('token');
                localStorage.removeItem('user');
            }
        }
    }, [navigate]);

    // Predefiniowane konta do szybkiego logowania
    const quickLoginAccounts = {
        Customer: { email: 'customer@test.com', password: 'Customer123!' },
        Agent: { email: 'agent@test.com', password: 'Agent123!' },
        Administrator: { email: 'admin@test.com', password: 'Admin123!' }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await authService.login({ email, password });
            
            localStorage.setItem('token', response.token);
            
            // Fetch user profile from UserService
            const user = await userService.getProfile();
            localStorage.setItem('user', JSON.stringify(user));
            
            // Sprawdź czy rola użytkownika zgadza się z wybraną
            if (user.role !== selectedRole) {
                setError(`To konto nie ma roli ${getRoleLabel(selectedRole)}. Zalogowano jako ${getRoleLabel(user.role)}.`);
            }

            // Przekieruj w zależności od roli
            switch (user.role) {
                case 'Administrator':
                    navigate('/admin', { replace: true });
                    break;
                case 'Agent':
                    navigate('/tickets', { replace: true });
                    break;
                case 'Customer':
                    navigate('/dashboard', { replace: true });
                    break;
                default:
                    navigate('/dashboard', { replace: true });
            }
        } catch (err: any) {
            console.error('Login failed:', err);
            setError(err.response?.data?.message || 'Nieprawidłowy email lub hasło');
        } finally {
            setLoading(false);
        }
    };

    const handleQuickLogin = (role: 'Customer' | 'Agent' | 'Administrator') => {
        const account = quickLoginAccounts[role];
        setEmail(account.email);
        setPassword(account.password);
        setSelectedRole(role);
    };

    const getRoleLabel = (role: string) => {
        switch (role) {
            case 'Administrator':
                return 'Administrator';
            case 'Agent':
                return 'Agent';
            case 'Customer':
                return 'Klient';
            default:
                return role;
        }
    };

    const getRoleIcon = (role: string) => {
        switch (role) {
            case 'Administrator':
                return <Shield className="h-5 w-5" />;
            case 'Agent':
                return <Users className="h-5 w-5" />;
            case 'Customer':
                return <UserCircle className="h-5 w-5" />;
            default:
                return <UserCircle className="h-5 w-5" />;
        }
    };

    const getRoleDescription = (role: string) => {
        switch (role) {
            case 'Administrator':
                return 'Zarządzanie użytkownikami i systemem';
            case 'Agent':
                return 'Rozwiązywanie zgłoszeń klientów';
            case 'Customer':
                return 'Tworzenie i śledzenie zgłoszeń';
            default:
                return '';
        }
    };

    return (
        <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 flex items-center justify-center p-4">
            <Card className="w-full max-w-5xl shadow-2xl">
                <CardHeader className="space-y-1 text-center">
                    <CardTitle className="text-3xl font-bold">Helpdesk System</CardTitle>
                    <CardDescription className="text-base">
                        Wybierz typ konta i zaloguj się
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                        {/* Lewa strona - Wybór roli */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold mb-4">Wybierz rolę:</h3>
                            
                            {/* Customer */}
                            <div
                                onClick={() => setSelectedRole('Customer')}
                                className={`p-4 border-2 rounded-lg cursor-pointer transition-all ${
                                    selectedRole === 'Customer'
                                        ? 'border-blue-500 bg-blue-50'
                                        : 'border-gray-200 hover:border-blue-300'
                                }`}
                            >
                                <div className="flex items-start gap-3">
                                    <div className={`p-2 rounded-full ${
                                        selectedRole === 'Customer' ? 'bg-blue-500 text-white' : 'bg-gray-200 text-gray-600'
                                    }`}>
                                        {getRoleIcon('Customer')}
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center justify-between">
                                            <h4 className="font-semibold text-gray-900">Klient</h4>
                                            {selectedRole === 'Customer' && (
                                                <div className="w-4 h-4 bg-blue-500 rounded-full"></div>
                                            )}
                                        </div>
                                        <p className="text-sm text-gray-600 mt-1">
                                            {getRoleDescription('Customer')}
                                        </p>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="mt-2"
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                handleQuickLogin('Customer');
                                            }}
                                        >
                                            Szybkie logowanie
                                        </Button>
                                    </div>
                                </div>
                            </div>

                            {/* Agent */}
                            <div
                                onClick={() => setSelectedRole('Agent')}
                                className={`p-4 border-2 rounded-lg cursor-pointer transition-all ${
                                    selectedRole === 'Agent'
                                        ? 'border-purple-500 bg-purple-50'
                                        : 'border-gray-200 hover:border-purple-300'
                                }`}
                            >
                                <div className="flex items-start gap-3">
                                    <div className={`p-2 rounded-full ${
                                        selectedRole === 'Agent' ? 'bg-purple-500 text-white' : 'bg-gray-200 text-gray-600'
                                    }`}>
                                        {getRoleIcon('Agent')}
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center justify-between">
                                            <h4 className="font-semibold text-gray-900">Agent</h4>
                                            {selectedRole === 'Agent' && (
                                                <div className="w-4 h-4 bg-purple-500 rounded-full"></div>
                                            )}
                                        </div>
                                        <p className="text-sm text-gray-600 mt-1">
                                            {getRoleDescription('Agent')}
                                        </p>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="mt-2"
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                handleQuickLogin('Agent');
                                            }}
                                        >
                                            Szybkie logowanie
                                        </Button>
                                    </div>
                                </div>
                            </div>

                            {/* Administrator */}
                            <div
                                onClick={() => setSelectedRole('Administrator')}
                                className={`p-4 border-2 rounded-lg cursor-pointer transition-all ${
                                    selectedRole === 'Administrator'
                                        ? 'border-red-500 bg-red-50'
                                        : 'border-gray-200 hover:border-red-300'
                                }`}
                            >
                                <div className="flex items-start gap-3">
                                    <div className={`p-2 rounded-full ${
                                        selectedRole === 'Administrator' ? 'bg-red-500 text-white' : 'bg-gray-200 text-gray-600'
                                    }`}>
                                        {getRoleIcon('Administrator')}
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center justify-between">
                                            <h4 className="font-semibold text-gray-900">Administrator</h4>
                                            {selectedRole === 'Administrator' && (
                                                <div className="w-4 h-4 bg-red-500 rounded-full"></div>
                                            )}
                                        </div>
                                        <p className="text-sm text-gray-600 mt-1">
                                            {getRoleDescription('Administrator')}
                                        </p>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="mt-2"
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                handleQuickLogin('Administrator');
                                            }}
                                        >
                                            Szybkie logowanie
                                        </Button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Prawa strona - Formularz logowania */}
                        <div className="space-y-4">
                            <h3 className="text-lg font-semibold mb-4">Dane logowania:</h3>
                            
                            {error && (
                                <Alert variant="destructive">
                                    <AlertDescription>{error}</AlertDescription>
                                </Alert>
                            )}

                            <form onSubmit={handleSubmit} className="space-y-4">
                                <div className="space-y-2">
                                    <Label htmlFor="email">Email</Label>
                                    <Input
                                        id="email"
                                        type="email"
                                        placeholder="twoj@email.com"
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        required
                                        disabled={loading}
                                    />
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="password">Hasło</Label>
                                    <Input
                                        id="password"
                                        type="password"
                                        placeholder="••••••••"
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        required
                                        disabled={loading}
                                    />
                                </div>

                                <div className="p-4 bg-gray-50 rounded-lg">
                                    <p className="text-sm text-gray-600 mb-2">
                                        <strong>Wybrany typ konta:</strong>
                                    </p>
                                    <div className="flex items-center gap-2">
                                        {getRoleIcon(selectedRole)}
                                        <span className="font-semibold">{getRoleLabel(selectedRole)}</span>
                                    </div>
                                    <p className="text-xs text-gray-500 mt-1">
                                        {getRoleDescription(selectedRole)}
                                    </p>
                                </div>

                                <Button
                                    type="submit"
                                    className="w-full"
                                    disabled={loading}
                                >
                                    {loading ? (
                                        <>
                                            <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                                            Logowanie...
                                        </>
                                    ) : (
                                        <>
                                            <LogIn className="mr-2 h-4 w-4" />
                                            Zaloguj się
                                        </>
                                    )}
                                </Button>
                            </form>

                            <div className="text-center pt-4 border-t">
                                <p className="text-sm text-gray-600">
                                    Nie masz konta?{' '}
                                    <Link
                                        to="/register"
                                        className="text-blue-600 hover:underline font-semibold"
                                    >
                                        Zarejestruj się
                                    </Link>
                                </p>
                            </div>

                            {/* Test credentials info */}
                            <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                                <p className="text-xs text-blue-800 font-semibold mb-2">
                                    💡 Konta testowe (kliknij "Szybkie logowanie"):
                                </p>
                                <div className="space-y-1 text-xs text-blue-700">
                                    <p>👤 Klient: customer@test.com / Customer123!</p>
                                    <p>👥 Agent: agent@test.com / Agent123!</p>
                                    <p>🛡️ Admin: admin@test.com / Admin123!</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
