import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { ArrowLeft, Save, AlertCircle } from 'lucide-react';
import { mockUsers } from '@/data/mockData';
import type { User } from '@/types/user.types';
import Layout from '@/components/ui/Layout';

export default function EditUserPage() {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const [user, setUser] = useState<User | null>(null);
    const [formData, setFormData] = useState({
        firstName: '',
        lastName: '',
        phoneNumber: '',
        role: 'Customer',
        isActive: true,
    });
    const [errors, setErrors] = useState<Record<string, string>>({});
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        loadUser();
    }, [id]);

    const loadUser = async () => {
        if (!id) return;
        
        try {
            setLoading(true);
            // TODO: Replace with real API call
            // const data = await userService.getUserById(id);
            const data = mockUsers.find(u => u.id === id);
            
            if (data) {
                setUser(data as User);
                setFormData({
                    firstName: data.firstName,
                    lastName: data.lastName,
                    phoneNumber: data.phoneNumber || '',
                    role: data.role,
                    isActive: data.isActive,
                });
            }
        } catch (error) {
            console.error('Failed to load user:', error);
        } finally {
            setLoading(false);
        }
    };

    const validateForm = () => {
        const newErrors: Record<string, string> = {};

        if (!formData.firstName) {
            newErrors.firstName = 'Imię jest wymagane';
        }

        if (!formData.lastName) {
            newErrors.lastName = 'Nazwisko jest wymagane';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!validateForm() || !id) {
            return;
        }

        try {
            setSaving(true);
            // TODO: Replace with real API call
            // await userService.updateUser(id, formData);
            console.log('Updating user:', id, formData);
            
            // Simulate API call
            await new Promise(resolve => setTimeout(resolve, 1000));
            
            navigate('/users');
        } catch (error) {
            console.error('Failed to update user:', error);
            setErrors({ submit: 'Nie udało się zaktualizować użytkownika' });
        } finally {
            setSaving(false);
        }
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value, type } = e.target;
        const finalValue = type === 'checkbox' ? (e.target as HTMLInputElement).checked : value;
        
        setFormData(prev => ({ ...prev, [name]: finalValue }));
        
        if (errors[name]) {
            setErrors(prev => ({ ...prev, [name]: '' }));
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Ładowanie danych użytkownika...</p>
                </div>
            </div>
        );
    }

    if (!user) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 flex items-center justify-center">
                <Card className="max-w-md">
                    <CardContent className="pt-6 text-center">
                        <AlertCircle className="mx-auto h-12 w-12 text-red-500 mb-4" />
                        <h2 className="text-xl font-semibold mb-2">Nie znaleziono użytkownika</h2>
                        <p className="text-gray-600 mb-4">Użytkownik o ID {id} nie istnieje.</p>
                        <Button onClick={() => navigate('/users')}>
                            <ArrowLeft className="mr-2 h-4 w-4" />
                            Powrót do listy
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <Layout currentPage="/users">
            <main className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <Button 
                    variant="ghost" 
                    onClick={() => navigate('/users')} 
                    className="mb-6"
                >
                    <ArrowLeft className="mr-2 h-4 w-4" />
                    Powrót do listy użytkowników
                </Button>

                <Card>
                    <CardHeader>
                        <div className="flex items-center justify-between">
                            <div>
                                <CardTitle>Edytuj użytkownika</CardTitle>
                                <CardDescription>
                                    Zaktualizuj dane użytkownika {user.fullName}
                                </CardDescription>
                            </div>
                            <Badge variant="outline">ID: {user.id}</Badge>
                        </div>
                    </CardHeader>
                    <CardContent>
                        <form onSubmit={handleSubmit} className="space-y-6">
                            {/* Email (read-only) */}
                            <div className="space-y-2">
                                <Label htmlFor="email">Email</Label>
                                <Input
                                    id="email"
                                    type="email"
                                    value={user.email}
                                    disabled
                                    className="bg-gray-100"
                                />
                                <p className="text-xs text-gray-500">Email nie może być zmieniony</p>
                            </div>

                            {/* First Name & Last Name */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="space-y-2">
                                    <Label htmlFor="firstName">
                                        Imię <span className="text-red-500">*</span>
                                    </Label>
                                    <Input
                                        id="firstName"
                                        name="firstName"
                                        type="text"
                                        value={formData.firstName}
                                        onChange={handleChange}
                                        placeholder="Jan"
                                        className={errors.firstName ? 'border-red-500' : ''}
                                    />
                                    {errors.firstName && (
                                        <p className="text-sm text-red-500">{errors.firstName}</p>
                                    )}
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="lastName">
                                        Nazwisko <span className="text-red-500">*</span>
                                    </Label>
                                    <Input
                                        id="lastName"
                                        name="lastName"
                                        type="text"
                                        value={formData.lastName}
                                        onChange={handleChange}
                                        placeholder="Kowalski"
                                        className={errors.lastName ? 'border-red-500' : ''}
                                    />
                                    {errors.lastName && (
                                        <p className="text-sm text-red-500">{errors.lastName}</p>
                                    )}
                                </div>
                            </div>

                            {/* Phone Number */}
                            <div className="space-y-2">
                                <Label htmlFor="phoneNumber">Numer telefonu</Label>
                                <Input
                                    id="phoneNumber"
                                    name="phoneNumber"
                                    type="tel"
                                    value={formData.phoneNumber}
                                    onChange={handleChange}
                                    placeholder="+48 123 456 789"
                                />
                            </div>

                            {/* Role */}
                            <div className="space-y-2">
                                <Label htmlFor="role">
                                    Rola <span className="text-red-500">*</span>
                                </Label>
                                <select
                                    id="role"
                                    name="role"
                                    value={formData.role}
                                    onChange={handleChange}
                                    className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                                >
                                    <option value="Customer">Klient</option>
                                    <option value="Agent">Agent</option>
                                    <option value="Administrator">Administrator</option>
                                </select>
                            </div>

                            {/* Active Status */}
                            <div className="flex items-center space-x-2">
                                <input
                                    type="checkbox"
                                    id="isActive"
                                    name="isActive"
                                    checked={formData.isActive}
                                    onChange={handleChange}
                                    className="w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
                                />
                                <Label htmlFor="isActive" className="font-normal cursor-pointer">
                                    Konto aktywne
                                </Label>
                            </div>

                            {/* Metadata */}
                            <div className="bg-gray-50 p-4 rounded-md space-y-2 text-sm">
                                <div className="flex justify-between">
                                    <span className="text-gray-600">Data utworzenia:</span>
                                    <span className="font-medium">
                                        {new Date(user.createdAt).toLocaleString('pl-PL')}
                                    </span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-600">Ostatnia aktualizacja:</span>
                                    <span className="font-medium">
                                        {new Date(user.updatedAt).toLocaleString('pl-PL')}
                                    </span>
                                </div>
                            </div>

                            {/* Submit Error */}
                            {errors.submit && (
                                <div className="p-3 bg-red-50 border border-red-200 rounded-md">
                                    <p className="text-sm text-red-600">{errors.submit}</p>
                                </div>
                            )}

                            {/* Actions */}
                            <div className="flex gap-4 pt-4">
                                <Button
                                    type="submit"
                                    disabled={saving}
                                    className="flex-1"
                                >
                                    {saving ? (
                                        <>
                                            <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                                            Zapisywanie...
                                        </>
                                    ) : (
                                        <>
                                            <Save className="mr-2 h-4 w-4" />
                                            Zapisz zmiany
                                        </>
                                    )}
                                </Button>
                                <Button
                                    type="button"
                                    variant="outline"
                                    onClick={() => navigate('/users')}
                                    disabled={saving}
                                >
                                    Anuluj
                                </Button>
                            </div>
                        </form>
                    </CardContent>
                </Card>
            </main>
        </Layout>
    );
}
