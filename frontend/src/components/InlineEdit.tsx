import { useState, useRef, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Loader2, Pencil } from 'lucide-react';
import { cn } from '@/lib/utils';

interface InlineEditProps {
  value: string;
  onSave: (newValue: string) => Promise<void>;
  isEditable: boolean;
  multiline?: boolean;
  className?: string;
  placeholder?: string;
}

export default function InlineEdit({
  value,
  onSave,
  isEditable,
  multiline = false,
  className,
  placeholder = 'Kliknij aby edytować'
}: InlineEditProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editValue, setEditValue] = useState(value);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement | HTMLTextAreaElement>(null);

  useEffect(() => {
    setEditValue(value);
  }, [value]);

  useEffect(() => {
    if (isEditing && inputRef.current) {
      inputRef.current.focus();
      inputRef.current.select();
    }
  }, [isEditing]);

  const handleSave = async () => {
    if (editValue.trim() === value.trim()) {
      setIsEditing(false);
      return;
    }

    if (!editValue.trim()) {
      setError('Pole nie może być puste');
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      await onSave(editValue.trim());
      setIsEditing(false);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Nie udało się zapisać zmian');
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    setEditValue(value);
    setIsEditing(false);
    setError(null);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      handleCancel();
    } else if (e.key === 'Enter' && !multiline) {
      e.preventDefault();
      handleSave();
    } else if (e.key === 'Enter' && e.ctrlKey && multiline) {
      e.preventDefault();
      handleSave();
    }
  };

  if (!isEditable) {
    return multiline ? (
      <p className={cn("whitespace-pre-wrap", className)}>{value}</p>
    ) : (
      <span className={className}>{value}</span>
    );
  }

  if (isEditing) {
    return (
      <div className="relative">
        {multiline ? (
          <Textarea
            ref={inputRef as React.RefObject<HTMLTextAreaElement>}
            value={editValue}
            onChange={(e) => setEditValue(e.target.value)}
            onBlur={handleSave}
            onKeyDown={handleKeyDown}
            disabled={isSaving}
            placeholder={placeholder}
            className={cn("min-h-[100px]", error && "border-red-500")}
          />
        ) : (
          <Input
            ref={inputRef as React.RefObject<HTMLInputElement>}
            value={editValue}
            onChange={(e) => setEditValue(e.target.value)}
            onBlur={handleSave}
            onKeyDown={handleKeyDown}
            disabled={isSaving}
            placeholder={placeholder}
            className={cn(error && "border-red-500")}
          />
        )}
        {isSaving && (
          <div className="absolute right-2 top-1/2 -translate-y-1/2">
            <Loader2 className="h-4 w-4 animate-spin text-blue-600" />
          </div>
        )}
        {error && (
          <p className="text-sm text-red-500 mt-1">{error}</p>
        )}
        {multiline && (
          <p className="text-xs text-gray-400 mt-1">Ctrl+Enter aby zapisać, Escape aby anulować</p>
        )}
      </div>
    );
  }

  return (
    <div
      onClick={() => setIsEditing(true)}
      className={cn(
        "group cursor-pointer hover:bg-gray-50 rounded px-2 py-1 -mx-2 -my-1 transition-colors",
        className
      )}
    >
      {multiline ? (
        <p className="whitespace-pre-wrap">{value || <span className="text-gray-400">{placeholder}</span>}</p>
      ) : (
        <span>{value || <span className="text-gray-400">{placeholder}</span>}</span>
      )}
      <Pencil className="inline-block ml-2 h-4 w-4 text-gray-400 opacity-0 group-hover:opacity-100 transition-opacity" />
    </div>
  );
}
