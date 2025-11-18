import { Navigate } from 'react-router-dom';
import type { ReactNode } from 'react';

interface ProtectedRouteProps {
    children: ReactNode;
    requiredRole?: 'Customer' | 'Agent' | 'Administrator';
}

export default function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
    const token = localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    if (!token || !user.id) {
        return <Navigate to="/login" replace />;
    }

    if (requiredRole) {
        if (user.role === 'Administrator') {
            return <>{children}</>;
        }

        if (requiredRole === 'Agent' && user.role === 'Agent') {
            return <>{children}</>;
        }

        if (requiredRole === 'Customer' && user.role === 'Customer') {
            return <>{children}</>;
        }

        return <Navigate to="/dashboard" replace />;
    }

    return <>{children}</>;
}