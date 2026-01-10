import { useState, useEffect } from 'react';
import { ticketService } from '@/services/ticketService';
import type { TicketHistoryEntry } from '@/types/ticket.types';
import { Loader2, Clock, ArrowRight, AlertCircle } from 'lucide-react';

interface TicketHistoryProps {
  ticketId: string;
}

const actionLabels: Record<string, string> = {
  'Created': 'Utworzono zgłoszenie',
  'Updated': 'Zaktualizowano zgłoszenie',
  'StatusChanged': 'Zmieniono status',
  'PriorityChanged': 'Zmieniono priorytet',
  'Assigned': 'Przypisano agenta',
  'Unassigned': 'Usunięto przypisanie',
  'CommentAdded': 'Dodano komentarz',
  'AttachmentAdded': 'Dodano załącznik',
  'AttachmentRemoved': 'Usunięto załącznik',
  'Closed': 'Zamknięto zgłoszenie',
  'Reopened': 'Ponownie otwarto zgłoszenie',
};

const statusLabels: Record<string, string> = {
  'New': 'Nowe',
  'Open': 'Otwarte',
  'InProgress': 'W trakcie',
  'Pending': 'Oczekujące',
  'Resolved': 'Rozwiązane',
  'Closed': 'Zamknięte',
  'Assigned': 'Przypisane',
};

const priorityLabels: Record<string, string> = {
  'Critical': 'Krytyczny',
  'High': 'Wysoki',
  'Medium': 'Średni',
  'Low': 'Niski',
};

function formatValue(fieldName: string | undefined, value: string | undefined): string {
  if (!value) return '—';

  // Try to parse as JSON first (backend sometimes stores values as JSON)
  try {
    const parsed = JSON.parse(value);
    if (typeof parsed === 'string') {
      value = parsed;
    }
  } catch {
    // Not JSON, use as-is
  }

  if (fieldName === 'Status') {
    return statusLabels[value] || value;
  }
  if (fieldName === 'Priority') {
    return priorityLabels[value] || value;
  }

  return value;
}

export default function TicketHistory({ ticketId }: TicketHistoryProps) {
  const [history, setHistory] = useState<TicketHistoryEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadHistory();
  }, [ticketId]);

  const loadHistory = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await ticketService.getTicketHistory(ticketId);
      setHistory(data);
    } catch (err: any) {
      console.error('Failed to load ticket history:', err);
      setError(err.response?.data?.message || 'Nie udało się załadować historii');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-8">
        <Loader2 className="h-6 w-6 animate-spin text-blue-600" />
        <span className="ml-2 text-gray-600">Ładowanie historii...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center gap-2 text-red-600 py-4">
        <AlertCircle className="h-5 w-5" />
        <span>{error}</span>
      </div>
    );
  }

  if (history.length === 0) {
    return (
      <p className="text-gray-500 text-center py-4">Brak historii zmian</p>
    );
  }

  return (
    <div className="space-y-3 max-h-96 overflow-y-auto">
      {history.map((entry) => (
        <div
          key={entry.id}
          className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg"
        >
          <div className="mt-0.5">
            <Clock className="h-4 w-4 text-gray-400" />
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-gray-900">
              {actionLabels[entry.action] || entry.action}
            </p>
            {(entry.oldValue || entry.newValue) && (
              <p className="text-sm text-gray-600 mt-1 flex items-center gap-2 flex-wrap">
                <span className="text-gray-500 line-through">
                  {formatValue(entry.fieldName, entry.oldValue)}
                </span>
                <ArrowRight className="h-3 w-3 text-gray-400 flex-shrink-0" />
                <span className="font-medium">
                  {formatValue(entry.fieldName, entry.newValue)}
                </span>
              </p>
            )}
            {entry.description && (
              <p className="text-sm text-gray-500 mt-1">{entry.description}</p>
            )}
            <p className="text-xs text-gray-400 mt-1">
              {new Date(entry.createdAt).toLocaleString('pl-PL', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
              })}
            </p>
          </div>
        </div>
      ))}
    </div>
  );
}
