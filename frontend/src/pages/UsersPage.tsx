import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
//import { mockUsers } from "@/data/mockData";
import { Users, UserPlus, Search, Edit, Trash2, CheckCircle, XCircle} from "lucide-react";
import Layout from "@/components/ui/Layout";
import { userService } from "@/services/userService";
import type { User} from "@/types/user.types";
export default function UsersPage(){
    const navigate = useNavigate();
    const [users,setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);

    const [searchQuery, setSearchQuery] = useState('');
    const [roleFilter, setRoleFilter] = useState<string>('all');

    // TEMPORARY: Commented out for testing
    // const user = JSON.parse(localStorage.getItem('user') || '{}');
    // const isAdmin = user.role === 'Administrator';

    // TEMPORARY: Disable redirect for testing
    // useEffect(() => {
    //     if(!isAdmin)
    //     {
    //         navigate('/dashboard');
    //     }
    // }, [isAdmin, navigate]);

    useEffect(() => 
    {
        loadUsers();
    }, []);


    const loadUsers= async () => {
        try{
            setLoading(true);
            const response = await userService.getAllUsers();

            const data = (response as any).users || (response as any).items || response;

            if(Array.isArray(data))
            {
                setUsers(data);
            }else{
                setUsers([]);
                console.error("Otrzymano nieprawidłowy format danych: ",data);
            }
        }catch(error)
        {
            console.error("Nie udało się pobrać użytkowników: ",error);

        }finally
        {
            setLoading(false);
        }
    }


    const filteredUsers = users.filter( u => {

        const fullName = u.fullName || `${u.firstName} ${u.lastName}` || '';
        const email = u.email || '';

        const matchesSearch = 
        fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        email.toLowerCase().includes(searchQuery.toLowerCase());
        const matchesRole = roleFilter === 'all' || u.role === roleFilter;

        return matchesSearch && matchesRole;
    });

    const getRoleBadgeVariant = (role: string) => {
        switch (role) {
            case 'Administrator':
                return 'bg-red-500 text-white';
            case 'Agent':
                return 'bg-blue-500 text-white';
            case 'Customer':
                return 'bg-green-500 text-white';
            default:
                return 'bg-gray-500 text-white';
        }
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
    const handleDeleteUser = async (userId: string) => {
        if (!confirm('Czy na pewno chcesz usunąć tego użytkownika?')) {   
            return;
        }    
        try{
            await userService.deleteUser(userId);
            setUsers(users.filter(u => u.id != userId));
        }catch(error)
        {
            console.error("Błąd podczas usuwania użytkownika: ",error);
        }
    };

    const handleToggleActive = async (user:User) => {
        try{
            const newStatus = !user.isActive;
            await userService.updateUser(user.id,{isActive: newStatus});

            setUsers(users.map( u => u.id === user.id ? {...u, isActive: newStatus}: u));

        }catch(error)
        {
            console.error("Błąd aktualizacji statusu: ",error);
            alert("Nie udało się zmienić statusu użytkownika");
        }
    };

    return (
        <Layout currentPage="/users">
            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                
                <div className="flex justify-between items-center mb-6">
                    <div className="flex items-center gap-3">
                        <Users className="h-8 w-8 text-blue-600" />
                        <div>
                            <h1 className="text-3xl font-bold text-slate-900">Zarządzanie użytkownikami</h1>
                            <p className="text-gray-600 mt-1">
                                {loading ? "Ładowanie..." : `Znaleziono ${filteredUsers.length} użytkowników`}
                            </p>
                        </div>
                    </div>
                    <Button onClick={() => navigate('/users/create')}>
                        <UserPlus className="mr-2 h-4 w-4" />
                        Dodaj użytkownika
                    </Button>
                </div>

                <Card className="mb-6">
                    <CardHeader>
                        <CardTitle>Filtry</CardTitle>
                        <CardDescription>Wyszukaj i filtruj użytkowników</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Wyszukaj
                                </label>
                                <div className="relative">
                                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                                    <Input
                                        type="text"
                                        placeholder="Szukaj po imieniu, nazwisku lub email..."
                                        value={searchQuery}
                                        onChange={(e) => setSearchQuery(e.target.value)}
                                        className="pl-10"
                                    />
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Rola
                                </label>
                                <select
                                    value={roleFilter}
                                    onChange={(e) => setRoleFilter(e.target.value)}
                                    className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                                >
                                    <option value="all">Wszystkie role</option>
                                    <option value="Administrator">Administrator</option>
                                    <option value="Agent">Agent</option>
                                    <option value="Customer">Klient</option>
                                </select>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <Card>
                    <CardContent className="p-0">
                        {loading ? (
                            <div className="text-center py-12 text-gray-500">Ładowanie danych...</div>
                        ) : (
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>Użytkownik</TableHead>
                                        <TableHead>Email</TableHead>
                                        <TableHead>Telefon</TableHead>
                                        <TableHead>Rola</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead>Data utworzenia</TableHead>
                                        <TableHead className="text-right">Akcje</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {filteredUsers.length === 0 ? (
                                        <TableRow>
                                            <TableCell colSpan={7} className="text-center py-8 text-gray-500">
                                                Nie znaleziono użytkowników
                                            </TableCell>
                                        </TableRow>
                                    ) : (
                                        filteredUsers.map((user) => (
                                            <TableRow key={user.id} className="hover:bg-gray-50">
                                                <TableCell>
                                                    <div>
                                                        <div className="font-semibold text-gray-900">
                                                            {user.fullName}
                                                        </div>
                                                        <div className="text-sm text-gray-500">
                                                            ID: {user.id}
                                                        </div>
                                                    </div>
                                                </TableCell>
                                                <TableCell className="text-gray-700">
                                                    {user.email}
                                                </TableCell>
                                                <TableCell className="text-gray-700">
                                                    {user.phoneNumber || '-'}
                                                </TableCell>
                                                <TableCell>
                                                    <Badge className={getRoleBadgeVariant(user.role)}>
                                                        {getRoleLabel(user.role)}
                                                    </Badge>
                                                </TableCell>
                                                <TableCell>
                                                    {user.isActive ? (
                                                        <div className="flex items-center text-green-600">
                                                            <CheckCircle className="h-4 w-4 mr-1" />
                                                            Aktywny
                                                        </div>
                                                    ) : (
                                                        <div className="flex items-center text-red-600">
                                                            <XCircle className="h-4 w-4 mr-1" />
                                                            Nieaktywny
                                                        </div>
                                                    )}
                                                </TableCell>
                                                <TableCell className="text-gray-700">
                                                    {new Date(user.createdAt).toLocaleDateString('pl-PL')}
                                                </TableCell>
                                                <TableCell className="text-right">
                                                    <div className="flex justify-end gap-2">
                                                        <Button
                                                            variant="outline"
                                                            size="sm"
                                                            onClick={() => navigate(`/users/${user.id}/edit`)}
                                                        >
                                                            <Edit className="h-4 w-4" />
                                                        </Button>
                                                        <Button
                                                            variant={user.isActive ? "outline" : "default"}
                                                            size="sm"
                                                            onClick={() => handleToggleActive(user)}
                                                            title={user.isActive ? "Dezaktywuj" : "Aktywuj"}
                                                        >
                                                            {user.isActive ? (
                                                                <XCircle className="h-4 w-4" />
                                                            ) : (
                                                                <CheckCircle className="h-4 w-4" />
                                                            )}
                                                        </Button>
                                                        <Button
                                                            variant="destructive"
                                                            size="sm"
                                                            onClick={() => handleDeleteUser(user.id)}
                                                        >
                                                            <Trash2 className="h-4 w-4" />
                                                        </Button>
                                                    </div>
                                                </TableCell>
                                            </TableRow>
                                        ))
                                    )}
                                </TableBody>
                            </Table>
                        )}
                    </CardContent>
                </Card>
            </main>
        </Layout>
    );
}