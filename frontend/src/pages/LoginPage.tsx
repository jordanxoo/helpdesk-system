import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@radix-ui/react-label";
import {Card, CardContent, CardDescription,CardHeader, CardTitle} from '@/components/ui/card';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '@/services/authService';
import { AlertCircle } from 'lucide-react';

export default function LoginPage()
{
    const navigate = useNavigate();
    const [email,setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await authService.login({ email, password });
            
            localStorage.setItem('token', response.token);
            localStorage.setItem('user', JSON.stringify(response.user));
            
            if(response.user.role === "Administrator")
            {
                navigate('/admin');
            }else
            {
                navigate('/dashboard');
            }
        } catch (err: any) {
            console.error('Login failed:', err);
            setError(err.response?.data?.message || 'Nieprawidłowy email lub hasło');
        } finally {
            setLoading(false);
        }
    };
    return  (
        <div className = "min-h-screen flex items-center justify-center bg-gradient-to-br from slate-900 via-slate-800 to-slate-900 p4">
            <Card className ="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <CardTitle className = "text-2xl font-bold text-center">
                        HelpdeskSystem
                        </CardTitle>
                        <CardDescription className = "text-center">
                            Zaloguj się do swojego konta
                            </CardDescription>
                        </CardHeader>
                        <CardContent>
                         <form onSubmit = {handleSubmit} className="space-y-4">
                            <div className = "space-y-2">
                                <Label htmlFor="email">Email</Label>
                                <Input
                                 id="email"
                                 type="email"
                                 placeholder="jan.kowalski@example.com"
                                 value={email}
                                 onChange={(e) => setEmail(e.target.value)}
                                 required
                                 />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="password">Hasło</Label>
                                <Input 
                                id="password"
                                type="password"
                                placeholder="********"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                                />
                            </div>

                            {error && (
                                <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 p-3 rounded-md">
                                    <AlertCircle className="h-4 w-4" />
                                    <span>{error}</span>
                                </div>
                            )}

                            <Button type="submit" className="w-full" disabled={loading}>
                                {loading ? 'Logowanie...' : 'Zaloguj się'}
                            </Button>
                            <div className="text-center text-sm text-muted-foreground">Nie masz konta {' '}
                                <Link to="/register" className="text-primary hover:underline font-medium">
                                    Zarejestruj się</Link> 
                                    </div>
                         </form>
                         </CardContent>
                         </Card>
                 </div>
            
    );
}