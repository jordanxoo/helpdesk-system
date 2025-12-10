import React, { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@radix-ui/react-label";
import {Card, CardContent, CardDescription,CardHeader,CardTitle} from '@/components/ui/card';
import {Link, useNavigate} from 'react-router-dom';
import { authService } from '@/services/authService';
import { userService } from '@/services/userService';
import { AlertCircle, CheckCircle } from 'lucide-react';
import type { PasswordRequirements } from '@/types/auth.types';

export default function RegisterPage()
{
    const navigate = useNavigate();
    const [formData,setFormData] = useState({
        email: '',
        password: '',
        confirmPassword: ' ',
        firstName: ' ',
        lastName: ' ',
        phoneNumber: ' ',

    });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);
    const [passwordRequirements, setPasswordRequirements] = useState<PasswordRequirements | null>(null);

    useEffect(() => {
        const fetchPasswordRequirements = async () => {
            try {
                const requirements = await authService.getPasswordRequirements();
                setPasswordRequirements(requirements);
            } catch (err) {
                console.error('Failed to fetch password requirements:', err);
            }
        };
        fetchPasswordRequirements();
    }, []);

    const validatePassword = (password: string): string | null => {
        if (!passwordRequirements) {
            // Fallback validation if requirements not loaded
            if (password.length < 6) {
                return 'Hasło musi mieć co najmniej 6 znaków';
            }
            return null;
        }

        if (password.length < passwordRequirements.minimumLength) {
            return `Hasło musi mieć co najmniej ${passwordRequirements.minimumLength} znaków`;
        }
        if (passwordRequirements.requireLowercase && !/[a-z]/.test(password)) {
            return 'Hasło musi zawierać małą literę';
        }
        if (passwordRequirements.requireUppercase && !/[A-Z]/.test(password)) {
            return 'Hasło musi zawierać dużą literę';
        }
        if (passwordRequirements.requireDigit && !/\d/.test(password)) {
            return 'Hasło musi zawierać cyfrę';
        }
        if (passwordRequirements.requireNonAlphanumeric && !/[^a-zA-Z0-9]/.test(password)) {
            return 'Hasło musi zawierać znak specjalny';
        }
        return null;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (formData.password !== formData.confirmPassword) {
            setError('Hasła nie są identyczne');
            return;
        }

        const passwordError = validatePassword(formData.password);
        if (passwordError) {
            setError(passwordError);
            return;
        }

        setLoading(true);

        try {
            await authService.register({
                email: formData.email.trim(),
                password: formData.password,
                firstName: formData.firstName.trim(),
                lastName: formData.lastName.trim(),
                phoneNumber: formData.phoneNumber.trim(),
                role: 'Customer'
            });
            
            // Rejestracja udana - przekieruj na stronę logowania
            setSuccess(true);
            setTimeout(() => {
                navigate('/login', { 
                    state: { 
                        message: 'Konto utworzone pomyślnie! Zaloguj się aby kontynuować.' 
                    } 
                });
            }, 1500);
        } catch (err: any) {
            console.error('Registration failed:', err);
            setError(err.response?.data?.message || 'Nie udało się utworzyć konta');
        } finally {
            setLoading(false);
        }
    };
    const handleChange = (e:React.ChangeEvent<HTMLInputElement>) =>
    {
        setFormData({
            ...formData,
            [e.target.name] : e.target.value,
        });
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <CardTitle className="text-2xl font bold">
                        <CardDescription>
                            Wprowadź swoje dane, aby założyć konto w systemie
                        </CardDescription>
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div className="grid grid-cols-2 gap-4">
                            <div className="space-y-2">
                                <Label htmlFor="firstName">Imię</Label>
                                <Input
                                id="firstName"
                                name="firstName"
                                type="text"
                                placeholder="Jan"
                                value={formData.firstName}
                                onChange={handleChange}
                                required
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="lastName"> Nazwisko</Label>
                                <Input
                                id="lastName"
                                name="lastName"
                                type="text"
                                placeholder="Kowalski"
                                value={formData.lastName}
                                onChange={handleChange}
                                required
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="phoneNumber">Numer Telefonu</Label>
                                <Input
                                id="phoneNumber"
                                name="phoneNumber"
                                type="tel"
                                placeholder="+48 123 456 789"
                                value={formData.phoneNumber}
                                onChange={handleChange}
                                required
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="email">Email</Label>
                                <Input
                                id="email"
                                name="email"
                                type="email"
                                placeholder="jan.kowalski@example.com"
                                value={formData.email}
                                onChange={handleChange}
                                required
                                />
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="password">Hasło</Label>
                                <Input
                                id="password"
                                name="password"
                                type="password"
                                placeholder="********"
                                value={formData.password}
                                onChange={handleChange}
                                required
                                />
                                {passwordRequirements && (
                                    <p className="text-xs text-muted-foreground">
                                        {passwordRequirements.description}
                                    </p>
                                )}
                            </div>
                            <div className="space-y-2">
                                <Label htmlFor="confirmPassword">Potwierdź hasło</Label>
                                <Input
                                id="confirmPassword"
                                name="confirmPassword"
                                type="password"
                                placeholder="********"
                                value={formData.confirmPassword}
                                onChange={handleChange}
                                required
                                />
                            </div>

                            {error && (
                                <div className="flex items-center gap-2 text-sm text-red-600 bg-red-50 p-3 rounded-md col-span-2">
                                    <AlertCircle className="h-4 w-4" />
                                    <span>{error}</span>
                                </div>
                            )}

                            {success && (
                                <div className="flex items-center gap-2 text-sm text-green-600 bg-green-50 p-3 rounded-md col-span-2">
                                    <CheckCircle className="h-4 w-4" />
                                    <span>Konto utworzone pomyślnie! Przekierowywanie...</span>
                                </div>
                            )}

                            <Button type="submit" className="w-full col-span-2" disabled={loading || success}>
                                {loading ? 'Tworzenie konta...' : success ? 'Sukces!' : 'Zarejestruj się'}
                            </Button>

                            <div className="text-center text-sm text-gray-600">
                                Masz już konto? {' '}
                                <Link to="/login" className="text-blue-600 hover:underline font-medium">
                                Zaloguj się
                                </Link>
                            </div>
                        </div>
                    </form>
                </CardContent>
            </Card>
        </div>
    );
}