
import React, { useState, useEffect} from 'react';
import { useNavigate } from 'react-router-dom';
import {Card, CardContent, CardDescription,CardHeader,CardTitle} from '@/components/ui/card';
import { Button  } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {Label} from '@/components/ui/label';
import {Badge} from '@/components/ui/badge';
import {User, Save, ArrowLeft,Mail,Phone, Calendar, Shield} from 'lucide-react';
import type { User as UserType} from '@/types/user.types';
import Layout from '@/components/ui/Layout';
import { userService } from '@/services/userService';

export default function ProfilePage(){

    const navigate = useNavigate();
    const [user, setUser] = useState<UserType | null>(null);
    const [formData, setFormData] = useState({
        firstName: '',
        lastName: '',
        phoneNumber: '',
    });

    const [errors, setErrors] = useState<Record<string,string>>({});
    const [saving, setSaving] = useState(false);
    const [successMessage, setSuccessMessage] = useState('');


    useEffect(() => {
        loadProfile();
    }, []);

    const loadProfile = async () => {

        try{
            const data = await userService.getProfile();

            setUser(data);
            setFormData({
                firstName: data.firstName,
                lastName: data.lastName,
                phoneNumber: data.phoneNumber || ''
            });
        }catch(err)
        {
            console.error("Nie udało się pobrać profilu użytkownika: ",err);
        }
    };

    const validateForm = () =>{
        const newErrors: Record<string,string> = {};


        if(!formData.firstName)
        {
            newErrors.firstName = 'Imie jest wymagane';
        }
        if(!formData.lastName)
        {
            newErrors.lastName = 'Nazwisko jest wymagane';
        }
        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if(!validateForm())
        {
            return;
        }

        if(!user || !user.id)
        {
            console.error("Brak ID użytkownika");
            return;
        }

        try{
            setSaving(true);
            setSuccessMessage('');

            const updatedUserFromApi = await userService.updateProfile(user.id,{
                firstName: formData.firstName,
                lastName: formData.lastName,
                phoneNumber: formData.phoneNumber
            });

            const mergeUser = {
                ...user,
                ...updatedUserFromApi
            };

            localStorage.setItem('user',JSON.stringify(mergeUser));
            setUser(mergeUser);

            setSuccessMessage("Profil został zaktualizowany!");
            
        }catch(error)
        {
            console.error('Failed to update user: ',error);
        }finally
        {
            setSaving(false);
        }
    };
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const {name, value} = e.target;
        setFormData(prev => ( {...prev, [name]: value}));

        if(errors[name]){
            setErrors(prev => ( {...prev,[name]: ''}));
        }
    };

    const getRoleBadgeColor = (role: string) =>{
        switch(role)
        {
          case 'Administrator':
                return 'bg-red-500 text-white';
            case 'Agent':
                return 'bg-blue-500 text-white';
            case 'Customer':
                return 'bg-green-500 text-white';
            default:
                return 'bg-gray-500 text-white';
          
        }
    }
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

    if (!user) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Ładowanie profilu...</p>
                </div>
            </div>
        );
    }

    return (
        <Layout currentPage="/profile">
            <div className="max-w-4xl mx-auto">
                <Button 
                    variant="ghost" 
                    onClick={() => navigate('/dashboard')} 
                    className="mb-6"
                >
                    <ArrowLeft className="mr-2 h-4 w-4" />
                    Powrót do dashboardu
                </Button>

                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Profile Info Card */}
                    <Card className="lg:col-span-1">
                        <CardHeader>
                            <div className="flex flex-col items-center text-center">
                                <div className="w-24 h-24 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center mb-4">
                                    <User className="h-12 w-12 text-white" />
                                </div>
                                <CardTitle className="text-xl">{user.fullName}</CardTitle>
                                <CardDescription className="mt-2">
                                    <Badge className={getRoleBadgeColor(user.role)}>
                                        {getRoleLabel(user.role)}
                                    </Badge>
                                </CardDescription>
                            </div>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="flex items-center text-sm text-gray-600">
                                <Mail className="h-4 w-4 mr-2" />
                                <span className="truncate">{user.email}</span>
                            </div>
                            {user.phoneNumber && (
                                <div className="flex items-center text-sm text-gray-600">
                                    <Phone className="h-4 w-4 mr-2" />
                                    <span>{user.phoneNumber}</span>
                                </div>
                            )}
                            <div className="flex items-center text-sm text-gray-600">
                                <Shield className="h-4 w-4 mr-2" />
                                <span>ID: {user.id}</span>
                            </div>
                            <div className="flex items-center text-sm text-gray-600">
                                <Calendar className="h-4 w-4 mr-2" />
                                <span>
                                    Dołączył {new Date(user.createdAt).toLocaleDateString('pl-PL')}
                                </span>
                            </div>
                        </CardContent>
                    </Card>

                    {/* Edit Profile Form */}
                    <Card className="lg:col-span-2">
                        <CardHeader>
                            <CardTitle>Edytuj profil</CardTitle>
                            <CardDescription>
                                Zaktualizuj swoje dane osobowe
                            </CardDescription>
                        </CardHeader>
                        <CardContent>
                            <form onSubmit={handleSubmit} className="space-y-6">
                                {/* Success Message */}
                                {successMessage && (
                                    <div className="p-3 bg-green-50 border border-green-200 rounded-md">
                                        <p className="text-sm text-green-600">{successMessage}</p>
                                    </div>
                                )}

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
                                        onClick={() => navigate('/dashboard')}
                                        disabled={saving}
                                    >
                                        Anuluj
                                    </Button>
                                </div>
                            </form>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </Layout>
    );
}


