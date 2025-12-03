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
import ProtectedRoute from './components/ProtectedRoute';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/" element={<Navigate to="/login" replace />} />

        {/* Protected routes - accessible by all logged-in users */}
        <Route 
          path="/dashboard" 
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/tickets" 
          element={
            <ProtectedRoute>
              <TicketsPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/tickets/create" 
          element={
            <ProtectedRoute allowedRoles={['Customer', 'Agent']}>
              <CreateTicketPage />
            </ProtectedRoute>
          }
        />
        <Route 
          path="/tickets/:id" 
          element={
            <ProtectedRoute>
              <TicketDetailsPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/profile" 
          element={
            <ProtectedRoute>
              <ProfilePage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/notifications" 
          element={
            <ProtectedRoute>
              <NotificationsPage />
            </ProtectedRoute>
          } 
        />

        {/* Admin only routes */}
        <Route 
          path="/admin" 
          element={
            <ProtectedRoute allowedRoles={['Administrator']}>
              <AdminDashboard />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/users" 
          element={
            <ProtectedRoute allowedRoles={['Administrator']}>
              <UsersPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/users/create" 
          element={
            <ProtectedRoute allowedRoles={['Administrator']}>
              <CreateUserPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/users/:id/edit" 
          element={
            <ProtectedRoute allowedRoles={['Administrator']}>
              <EditUserPage />
            </ProtectedRoute>
          } 
        />
      </Routes>
    </BrowserRouter>
  );
}

export default App;