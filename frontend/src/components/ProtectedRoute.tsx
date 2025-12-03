import { Navigate } from 'react-router-dom';

interface ProtectedRouteProps {
    children: React.ReactNode;
    allowedRoles?: ('Customer' | 'Agent' | 'Administrator')[];
}

export default function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');

    if (!token) {
        return <Navigate to="/login" replace />;
    }

    if (allowedRoles && userStr) {
        try {
            const user = JSON.parse(userStr);
            if (!allowedRoles.includes(user.role)) {
                return <Navigate to="/dashboard" replace />;
            }
        } catch (error) {
            console.error('Failed to parse user data:', error);
            return <Navigate to="/login" replace />;
        }
    }

    return <>{children}</>;
}
