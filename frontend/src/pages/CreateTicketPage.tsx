import { useState, useRef, useEffect, useMemo } from "react";
import { useNavigate, Link } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ticketService } from "@/services/ticketService";
import { Paperclip, X, FileText, Image, File, ZoomIn } from "lucide-react";
import { FILE_UPLOAD_LIMITS, validateFile } from "@/constants/fileValidation";

export default function CreateTicketPage()
{
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [previewImage, setPreviewImage] = useState<string | null>(null);
    const [previewFileName, setPreviewFileName] = useState<string>('');

    // Helper function to check if file is an image (defined early for useMemo)
    const isImageFile = (file: File) => file.type.startsWith('image/');

    // Create blob URLs for image grid with proper cleanup
    const imageGridUrls = useMemo(() => {
        const urls = new Map<File, string>();
        selectedFiles.filter(isImageFile).forEach(file => {
            urls.set(file, URL.createObjectURL(file));
        });
        return urls;
    }, [selectedFiles]);

    // Cleanup blob URLs when component unmounts or selectedFiles change
    useEffect(() => {
        return () => {
            imageGridUrls.forEach(url => URL.revokeObjectURL(url));
        };
    }, [imageGridUrls]);

    // Cleanup preview image blob URL
    useEffect(() => {
        return () => {
            if (previewImage) {
                URL.revokeObjectURL(previewImage);
            }
        };
    }, [previewImage]);

    // Handle escape key for modal
    useEffect(() => {
        const handleEscape = (e: KeyboardEvent) => {
            if (e.key === 'Escape' && previewImage) {
                closeImagePreview();
            }
        };
        window.addEventListener('keydown', handleEscape);
        return () => window.removeEventListener('keydown', handleEscape);
    }, [previewImage]);

    const [formData,setFormData] = useState({
        title: '',
        description: '',
        priority: 'Medium',
        category: 'Hardware',
    });

    const [errors, setErrors] = useState<Record<string,string>>({});

    const validateForm = () => {
        const newErrors: Record<string,string> = {};

        if(!formData.title.trim())
        {
            newErrors.title = 'Tytuł jest wymagany';
        }
        else if(formData.title.length < 5)
        {
            newErrors.title = 'Tytuł musi mieć minimum 5 znaków';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
        const files = event.target.files;
        if (!files) return;

        const newFiles: File[] = [];
        
        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const validation = validateFile(file);
            
            if (!validation.isValid) {
                alert(validation.error);
                continue;
            }
            
            newFiles.push(file);
        }

        setSelectedFiles(prev => [...prev, ...newFiles]);
        
        // Reset input
        if (fileInputRef.current) {
            fileInputRef.current.value = '';
        }
    };

    const removeFile = (index: number) => {
        setSelectedFiles(prev => prev.filter((_, i) => i !== index));
    };

    const formatFileSize = (bytes: number): string => {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    };

    const getFileIcon = (file: File) => {
        if (file.type.startsWith('image/')) {
            return <Image className="h-4 w-4 text-blue-500" />;
        }
        if (file.type === 'application/pdf') {
            return <FileText className="h-4 w-4 text-red-500" />;
        }
        return <File className="h-4 w-4 text-gray-500" />;
    };

    const openImagePreview = (file: File) => {
        const url = URL.createObjectURL(file);
        setPreviewImage(url);
        setPreviewFileName(file.name);
    };

    const closeImagePreview = () => {
        if (previewImage) {
            URL.revokeObjectURL(previewImage);
        }
        setPreviewImage(null);
        setPreviewFileName('');
    };

    const handleSubmit = async (e : React.FormEvent) => {
        e.preventDefault();

        if(!validateForm())
        {
            return;
        }
        try 
        {
            setIsSubmitting(true);

            // Utwórz ticket
            const createdTicket = await ticketService.createTicket({
                title: formData.title,
                description: formData.description,
                priority: formData.priority,
                category: formData.category
            });

            // Prześlij załączniki jeśli są
            if (selectedFiles.length > 0) {
                const uploadResults = await Promise.allSettled(
                    selectedFiles.map(file => ticketService.uploadAttachment(createdTicket.id, file))
                );

                const failedUploads = uploadResults.filter(result => result.status === 'rejected');
                if (failedUploads.length > 0) {
                    const successCount = uploadResults.length - failedUploads.length;
                    console.error('Failed uploads:', failedUploads);
                    
                    if (successCount === 0) {
                        // All uploads failed - stay on page
                        alert('Wszystkie pliki nie zostały przesłane. Spróbuj ponownie lub usuń załączniki.');
                        return;
                    }
                    
                    // Partial failure - ask user for confirmation
                    const confirmNavigate = confirm(
                        `Przesłano ${successCount} z ${uploadResults.length} plików. ${failedUploads.length} nie powiodło się.\n\nZgłoszenie zostało utworzone. Czy chcesz przejść do listy zgłoszeń?`
                    );
                    if (!confirmNavigate) {
                        return;
                    }
                }
            }

            navigate('/tickets');
        }
        catch(error)
        {
            console.error('Failed to create ticket: ',error);
            alert('Nie udało się utworzyć zgłoszenia. Spróbuj ponownie.');
        }finally
        {
            setIsSubmitting(false);
        }
    };

    return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      {/* Navbar */}
      <nav className="bg-white border-b border-gray-200 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-8">
              <Link to="/dashboard" className="text-2xl font-bold text-slate-900">
                HelpdeskSystem
              </Link>
              <div className="hidden md:flex space-x-4">
                <Link to="/dashboard" className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium">
                  Dashboard
                </Link>
                <Link to="/tickets" className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium">
                  Zgłoszenia
                </Link>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <Button variant="outline" size="sm">
                Wyloguj
              </Button>
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-6">
          <Link to="/tickets" className="text-blue-600 hover:text-blue-800 text-sm mb-2 inline-flex items-center">
            ← Powrót do zgłoszeń
          </Link>
          <h1 className="text-3xl font-bold text-slate-900 mt-2">Nowe zgłoszenie</h1>
          <p className="text-gray-600 mt-1">
            Wypełnij formularz aby utworzyć nowe zgłoszenie
          </p>
        </div>

        {/* Form */}
        <Card>
          <CardHeader>
            <CardTitle>Informacje o zgłoszeniu</CardTitle>
            <CardDescription>
              Pola oznaczone * są wymagane
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Title */}
              <div className="space-y-2">
                <Label htmlFor="title">
                  Tytuł zgłoszenia *
                </Label>
                <Input
                  id="title"
                  type="text"
                  placeholder="Np. Problem z logowaniem do systemu"
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  className={errors.title ? 'border-red-500' : ''}
                />
                {errors.title && (
                  <p className="text-sm text-red-600">{errors.title}</p>
                )}
              </div>

              {/* Description */}
              <div className="space-y-2">
                <Label htmlFor="description">
                  Opis problemu *
                </Label>
                <Textarea
                  id="description"
                  placeholder="Opisz szczegółowo problem, który wystąpił..."
                  rows={6}
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className={errors.description ? 'border-red-500' : ''}
                />
                {errors.description && (
                  <p className="text-sm text-red-600">{errors.description}</p>
                )}
                <p className="text-xs text-gray-500">
                  Minimum 20 znaków. Obecnie: {formData.description.length}
                </p>
              </div>

              {/* Priority */}
              <div className="space-y-2">
                <Label htmlFor="priority">
                  Priorytet
                </Label>
                <Select 
                  value={formData.priority} 
                  onValueChange={(value) => setFormData({ ...formData, priority: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Wybierz priorytet" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Low">
                      <div className="flex items-center gap-2">
                        <span className="w-2 h-2 rounded-full bg-green-500"></span>
                        Niski
                      </div>
                    </SelectItem>
                    <SelectItem value="Medium">
                      <div className="flex items-center gap-2">
                        <span className="w-2 h-2 rounded-full bg-yellow-500"></span>
                        Średni
                      </div>
                    </SelectItem>
                    <SelectItem value="High">
                      <div className="flex items-center gap-2">
                        <span className="w-2 h-2 rounded-full bg-orange-500"></span>
                        Wysoki
                      </div>
                    </SelectItem>
                    <SelectItem value="Critical">
                      <div className="flex items-center gap-2">
                        <span className="w-2 h-2 rounded-full bg-red-500"></span>
                        Krytyczny
                      </div>
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* Category */}
              <div className="space-y-2">
                <Label htmlFor="category">
                  Kategoria
                </Label>
                <Select 
                  value={formData.category} 
                  onValueChange={(value) => setFormData({ ...formData, category: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Wybierz kategorię" />
                  </SelectTrigger>
                 <SelectContent>
               
                <SelectItem value="Hardware">Sprzęt (Hardware)</SelectItem>
                <SelectItem value="Software">Oprogramowanie (Software)</SelectItem>
                <SelectItem value="Network">Sieć (Network)</SelectItem>
                <SelectItem value="Security">Bezpieczeństwo</SelectItem>
                <SelectItem value="Account">Problemy z kontem</SelectItem>
                <SelectItem value="Other">Inne</SelectItem>
              </SelectContent>
                              </Select>
              </div>

              {/* Attachments */}
              <div className="space-y-2">
                <Label className="flex items-center gap-2">
                  <Paperclip className="h-4 w-4" />
                  Załączniki
                </Label>
                
                {/* Lista wybranych plików */}
                {selectedFiles.length > 0 && (
                  <div className="space-y-3 mb-3">
                    {/* Podgląd obrazków */}
                    {selectedFiles.filter(isImageFile).length > 0 && (
                      <div className="grid grid-cols-3 gap-2">
                        {selectedFiles.map((file, index) => 
                          isImageFile(file) && (
                            <div 
                              key={`${file.name}-${file.size}-${index}`}
                              className="relative group aspect-square rounded-lg overflow-hidden border bg-gray-100"
                            >
                              <img 
                                src={imageGridUrls.get(file) || ''} 
                                alt={file.name}
                                className="w-full h-full object-cover cursor-pointer transition-transform group-hover:scale-105"
                                onClick={() => openImagePreview(file)}
                              />
                              <div 
                                className="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-colors flex items-center justify-center cursor-pointer"
                                onClick={() => openImagePreview(file)}
                              >
                                <ZoomIn className="h-6 w-6 text-white opacity-0 group-hover:opacity-100 transition-opacity" />
                              </div>
                              {/* Przycisk usuwania */}
                              <button
                                type="button"
                                onClick={(e) => { e.stopPropagation(); removeFile(index); }}
                                className="absolute top-1 right-1 bg-red-500 hover:bg-red-600 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                              >
                                <X className="h-3 w-3" />
                              </button>
                              <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/60 to-transparent p-1">
                                <p className="text-white text-xs truncate">{file.name}</p>
                              </div>
                            </div>
                          )
                        )}
                      </div>
                    )}

                    {/* Lista pozostałych plików (nie-obrazki) */}
                    {selectedFiles.map((file, index) => 
                      !isImageFile(file) && (
                        <div 
                          key={`${file.name}-${file.size}-${index}`}
                          className="flex items-center justify-between p-2 bg-gray-50 rounded-lg border"
                        >
                          <div className="flex items-center gap-2">
                            {getFileIcon(file)}
                            <span className="text-sm text-gray-700 truncate max-w-[200px]">
                              {file.name}
                            </span>
                            <span className="text-xs text-gray-500">
                              ({formatFileSize(file.size)})
                            </span>
                          </div>
                          <button
                            type="button"
                            onClick={() => removeFile(index)}
                            className="text-gray-400 hover:text-red-500 transition-colors"
                          >
                            <X className="h-4 w-4" />
                          </button>
                        </div>
                      )
                    )}
                  </div>
                )}

                {/* Input do wyboru plików */}
                <input
                  type="file"
                  ref={fileInputRef}
                  onChange={handleFileSelect}
                  className="hidden"
                  id="file-upload"
                  accept={FILE_UPLOAD_LIMITS.ACCEPT_ATTRIBUTE}
                  multiple
                />
                <label htmlFor="file-upload">
                  <Button 
                    type="button"
                    variant="outline" 
                    className="w-full cursor-pointer"
                    onClick={() => fileInputRef.current?.click()}
                  >
                    <Paperclip className="mr-2 h-4 w-4" />
                    Dodaj załączniki
                  </Button>
                </label>
                <p className="text-xs text-gray-500">
                  Maksymalny rozmiar pliku: {FILE_UPLOAD_LIMITS.MAX_SIZE / 1024 / 1024} MB. Możesz dodać wiele plików.
                </p>
              </div>

              {/* Buttons */}
              <div className="flex justify-end gap-4 pt-4">
                <Button 
                  type="button" 
                  variant="outline"
                  onClick={() => navigate('/tickets')}
                  disabled={isSubmitting}
                >
                  Anuluj
                </Button>
                <Button 
                  type="submit"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Tworzenie...' : 'Utwórz zgłoszenie'}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>

        {/* Help Card */}
        <Card className="mt-6 bg-blue-50 border-blue-200">
          <CardHeader>
            <CardTitle className="text-blue-900 text-base">Wskazówki</CardTitle>
          </CardHeader>
          <CardContent className="text-sm text-blue-800 space-y-2">
            <p>• Opisz problem jak najdokładniej</p>
            <p>• Podaj kroki które doprowadziły do błędu</p>
            <p>• Dodaj screenshoty lub inne pliki pomocne w rozwiązaniu problemu</p>
            <p>• Wybierz odpowiedni priorytet</p>
          </CardContent>
        </Card>
      </main>

      {/* Modal podglądu obrazu */}
      {previewImage && (
        <div 
          className="fixed inset-0 bg-black/90 z-50 flex flex-col items-center justify-center p-4"
          onClick={closeImagePreview}
          role="dialog"
          aria-modal="true"
          aria-labelledby="preview-title"
        >
          {/* Header z info o pliku */}
          <div className="w-full max-w-4xl mb-4 flex items-center justify-between">
            <div className="text-white">
              <p id="preview-title" className="font-medium text-lg">{previewFileName}</p>
            </div>
            <button
              onClick={closeImagePreview}
              className="text-white hover:text-gray-300 transition-colors p-2"
              aria-label="Zamknij podgląd"
            >
              <X className="h-8 w-8" />
            </button>
          </div>
          
          {/* Obraz */}
          <img 
            src={previewImage}
            alt={previewFileName}
            className="max-w-4xl w-full h-auto max-h-[80vh] object-contain rounded-lg"
            onClick={(e) => e.stopPropagation()}
          />
        </div>
      )}
    </div>
  );
}