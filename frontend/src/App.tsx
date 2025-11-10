import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from '@/pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashBoardPage';
import TicketsPage from '@/pages/TicketsPage';
import CreateTicketPage from './pages/CreateTicketPage';

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
      </Routes>
    </BrowserRouter>
  );
}

export default App;