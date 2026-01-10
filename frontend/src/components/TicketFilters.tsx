import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Search, X } from 'lucide-react';
import type { TicketSearchFilter } from '@/types/ticket.types';

interface TicketFiltersProps {
  filters: TicketSearchFilter;
  onFiltersChange: (filters: TicketSearchFilter) => void;
  loading?: boolean;
}

const statusOptions = [
  { value: '', label: 'Wszystkie statusy' },
  { value: 'New', label: 'Nowe' },
  { value: 'Open', label: 'Otwarte' },
  { value: 'InProgress', label: 'W trakcie' },
  { value: 'Pending', label: 'Oczekujące' },
  { value: 'Resolved', label: 'Rozwiązane' },
  { value: 'Closed', label: 'Zamknięte' },
];

const priorityOptions = [
  { value: '', label: 'Wszystkie priorytety' },
  { value: 'Critical', label: 'Krytyczny' },
  { value: 'High', label: 'Wysoki' },
  { value: 'Medium', label: 'Średni' },
  { value: 'Low', label: 'Niski' },
];

const categoryOptions = [
  { value: '', label: 'Wszystkie kategorie' },
  { value: 'Hardware', label: 'Sprzęt' },
  { value: 'Software', label: 'Oprogramowanie' },
  { value: 'Network', label: 'Sieć' },
  { value: 'Security', label: 'Bezpieczeństwo' },
  { value: 'Account', label: 'Konto' },
  { value: 'Other', label: 'Inne' },
];

export default function TicketFilters({
  filters,
  onFiltersChange,
  loading = false
}: TicketFiltersProps) {
  const hasActiveFilters = !!(
    filters.searchTerm ||
    filters.status ||
    filters.priority ||
    filters.category
  );

  const handleClearFilters = () => {
    onFiltersChange({
      searchTerm: '',
      status: '',
      priority: '',
      category: '',
      page: 1,
      pageSize: filters.pageSize
    });
  };

  return (
    <div className="flex flex-wrap gap-3 items-end">
      <div className="flex-1 min-w-[200px]">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
          <Input
            type="text"
            placeholder="Szukaj po tytule lub opisie..."
            value={filters.searchTerm || ''}
            onChange={(e) => onFiltersChange({ ...filters, searchTerm: e.target.value, page: 1 })}
            className="pl-10"
            disabled={loading}
          />
        </div>
      </div>

      <div className="w-[160px]">
        <Select
          value={filters.status || ''}
          onValueChange={(value) => onFiltersChange({ ...filters, status: value, page: 1 })}
          disabled={loading}
        >
          <SelectTrigger>
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            {statusOptions.map((option) => (
              <SelectItem key={option.value || 'all-status'} value={option.value || 'all'}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="w-[160px]">
        <Select
          value={filters.priority || ''}
          onValueChange={(value) => onFiltersChange({ ...filters, priority: value, page: 1 })}
          disabled={loading}
        >
          <SelectTrigger>
            <SelectValue placeholder="Priorytet" />
          </SelectTrigger>
          <SelectContent>
            {priorityOptions.map((option) => (
              <SelectItem key={option.value || 'all-priority'} value={option.value || 'all'}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="w-[160px]">
        <Select
          value={filters.category || ''}
          onValueChange={(value) => onFiltersChange({ ...filters, category: value, page: 1 })}
          disabled={loading}
        >
          <SelectTrigger>
            <SelectValue placeholder="Kategoria" />
          </SelectTrigger>
          <SelectContent>
            {categoryOptions.map((option) => (
              <SelectItem key={option.value || 'all-category'} value={option.value || 'all'}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {hasActiveFilters && (
        <Button
          variant="outline"
          size="sm"
          onClick={handleClearFilters}
          disabled={loading}
        >
          <X className="h-4 w-4 mr-1" />
          Wyczyść
        </Button>
      )}
    </div>
  );
}
