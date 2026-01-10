import { Button } from '@/components/ui/button';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  loading?: boolean;
}

export default function Pagination({
  currentPage,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
  loading = false
}: PaginationProps) {
  if (totalPages <= 1) {
    return null;
  }

  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalCount);

  return (
    <div className="flex items-center justify-between mt-6 pt-4 border-t border-gray-200">
      <p className="text-sm text-gray-600">
        Wyświetlanie {startItem}-{endItem} z {totalCount} zgłoszeń
      </p>

      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="sm"
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage <= 1 || loading}
        >
          <ChevronLeft className="h-4 w-4 mr-1" />
          Poprzednia
        </Button>

        <span className="text-sm text-gray-600 px-2">
          Strona {currentPage} z {totalPages}
        </span>

        <Button
          variant="outline"
          size="sm"
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage >= totalPages || loading}
        >
          Następna
          <ChevronRight className="h-4 w-4 ml-1" />
        </Button>
      </div>
    </div>
  );
}
