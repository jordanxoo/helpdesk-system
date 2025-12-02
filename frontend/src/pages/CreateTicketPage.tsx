import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ticketService } from "@/services/ticketService";

export default function CreateTicketPage()
{
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);


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

    const handleSubmit = async (e : React.FormEvent) => {
        e.preventDefault();

        if(!validateForm())
        {
            return;
        }
        try 
        {
            setIsSubmitting(true);

           await ticketService.createTicket({
            title: formData.title,
            description: formData.description,
            priority: formData.priority,
            category: formData.category
           } as any);

            console.log('Ticket created: ', formData);

            navigate('/tickets');
        }
        catch(error)
        {
            console.error('Failed to create ticket: ',error);
            // alert('Nie udało się utworzyć zgłoszenia. Sprobuj ponownie.');
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
            <CardTitle className="text-blue-900 text-base">💡 Wskazówki</CardTitle>
          </CardHeader>
          <CardContent className="text-sm text-blue-800 space-y-2">
            <p>• Opisz problem jak najdokładniej</p>
            <p>• Podaj kroki które doprowadziły do błędu</p>
            <p>• Dodaj screenshoty jeśli to możliwe (później)</p>
            <p>• Wybierz odpowiedni priorytet</p>
          </CardContent>
        </Card>
      </main>
    </div>
  );


}