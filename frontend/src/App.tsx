import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from '@/pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashBoardPage';
import TicketsPage from '@/pages/TicketsPage';
import CreateTicketPage from './pages/CreateTicketPage';
import TicketDetailsPage from './pages/TicketDetailsPage';
import UsersPage from './pages/UsersPage';
import CreateUserPage from './pages/CreateUserPage';
import EditUserPage from './pages/EditUserPage';
import ProfilePage from './pages/ProfilePage';
import AdminDashboard from './pages/AdminDashboard';
import NotificationsPage from './pages/NotificationsPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/tickets" element={<TicketsPage />} />
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/tickets/create" element={<CreateTicketPage />}/>
        <Route path="/tickets/:id" element={<TicketDetailsPage />} />
        <Route path="/users" element={<UsersPage />} />
        <Route path="/users/create" element={<CreateUserPage />} />
        <Route path="/users/:id/edit" element={<EditUserPage />} />
        <Route path="/profile" element={<ProfilePage />} />
        <Route path="/admin" element={<AdminDashboard />} />
        <Route path="/notifications" element={<NotificationsPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;