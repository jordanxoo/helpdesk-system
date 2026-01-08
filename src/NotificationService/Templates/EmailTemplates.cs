namespace NotificationService.Templates;

/// <summary>
/// Modern, professional email templates for the helpdesk system
/// </summary>
public static class EmailTemplates
{
    private const string BaseTemplate = @"
<!DOCTYPE html>
<html lang='pl'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #1f2937;
            background-color: #f3f4f6;
            padding: 20px;
        }}

        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}

        .email-header {{
            background: {headerGradient};
            color: white;
            padding: 30px;
            text-align: center;
        }}

        .email-header h1 {{
            font-size: 24px;
            font-weight: 600;
            margin-bottom: 8px;
        }}

        .email-header .icon {{
            font-size: 48px;
            margin-bottom: 10px;
        }}

        .email-body {{
            padding: 40px 30px;
        }}

        .greeting {{
            font-size: 18px;
            color: #111827;
            margin-bottom: 20px;
        }}

        .alert-box {{
            background: {alertBackground};
            border-left: 4px solid {alertBorderColor};
            padding: 20px;
            margin: 25px 0;
            border-radius: 6px;
        }}

        .alert-box h2 {{
            color: {alertTitleColor};
            font-size: 20px;
            margin-bottom: 10px;
            font-weight: 600;
        }}

        .alert-box p {{
            color: #374151;
            margin: 8px 0;
        }}

        .info-box {{
            background: #f0f9ff;
            border-left: 4px solid #3b82f6;
            padding: 20px;
            margin: 25px 0;
            border-radius: 6px;
        }}

        .info-box strong {{
            color: #1e40af;
        }}

        .info-box ul {{
            list-style: none;
            margin: 15px 0;
        }}

        .info-box li {{
            padding: 8px 0;
            color: #1f2937;
            border-bottom: 1px solid #dbeafe;
        }}

        .info-box li:last-child {{
            border-bottom: none;
        }}

        .info-box li strong {{
            color: #1e40af;
            display: inline-block;
            min-width: 140px;
        }}

        .cta-button {{
            display: inline-block;
            background: {buttonBackground};
            color: white;
            padding: 14px 32px;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            margin: 25px 0;
            transition: transform 0.2s, box-shadow 0.2s;
        }}

        .cta-button:hover {{
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }}

        .email-footer {{
            background: #f9fafb;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e5e7eb;
        }}

        .email-footer p {{
            color: #6b7280;
            font-size: 14px;
            margin: 8px 0;
        }}

        .brand {{
            color: #3b82f6;
            font-weight: 600;
        }}

        .divider {{
            height: 1px;
            background: #e5e7eb;
            margin: 30px 0;
        }}

        @media only screen and (max-width: 600px) {{
            .email-body {{
                padding: 30px 20px;
            }}

            .email-header {{
                padding: 25px 20px;
            }}

            .cta-button {{
                display: block;
                text-align: center;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <div class='icon'>{headerIcon}</div>
            <h1>{headerTitle}</h1>
            <p>{headerSubtitle}</p>
        </div>

        <div class='email-body'>
            {bodyContent}
        </div>

        <div class='email-footer'>
            <p>To jest automatyczna wiadomość z systemu <span class='brand'>Helpdesk</span></p>
            <p>Jeśli masz pytania, skontaktuj się z naszym zespołem wsparcia.</p>
            <p style='margin-top: 20px; font-size: 12px;'>© 2026 Helpdesk System. Wszystkie prawa zastrzeżone.</p>
        </div>
    </div>
</body>
</html>";

    public static string TicketReminder(string firstName, string title, Guid ticketId, int hoursSinceCreated, double daysSinceCreated)
    {
        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <div class='alert-box'>
                <h2>⏰ {title}</h2>
                <p>Twój ticket oczekuje na odpowiedź już <strong>{hoursSinceCreated} godzin</strong> ({daysSinceCreated:F1} dni).</p>
            </div>

            <p>Zauważyliśmy, że Twoje zgłoszenie wciąż jest otwarte i nie otrzymało jeszcze odpowiedzi.</p>

            <div class='info-box'>
                <strong>📋 Szczegóły zgłoszenia:</strong>
                <ul>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Status:</strong> Otwarty</li>
                    <li><strong>Czas oczekiwania:</strong> {hoursSinceCreated}h ({daysSinceCreated:F1} dni)</li>
                </ul>
            </div>

            <p><strong>Co możesz zrobić?</strong></p>
            <ul style='margin-left: 20px; margin-top: 15px; color: #374151;'>
                <li>Dodać więcej informacji do zgłoszenia</li>
                <li>Sprawdzić czy problem nadal występuje</li>
                <li>Zamknąć ticket jeśli problem został rozwiązany</li>
            </ul>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Zobacz Ticket
                </a>
            </center>

            <p style='color: #6b7280; font-size: 14px; margin-top: 30px;'>
                Jesteśmy tutaj, aby pomóc! Jeśli potrzebujesz wsparcia, nasz zespół jest gotowy do działania.
            </p>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)")
            .Replace("{headerIcon}", "⏰")
            .Replace("{headerTitle}", "Przypomnienie o Tickecie")
            .Replace("{headerSubtitle}", "Twoje zgłoszenie oczekuje na uwagę")
            .Replace("{alertBackground}", "#fef3c7")
            .Replace("{alertBorderColor}", "#f59e0b")
            .Replace("{alertTitleColor}", "#92400e")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)")
            .Replace("{bodyContent}", bodyContent);
    }

    public static string TicketReminderAgent(string firstName, string title, Guid ticketId, int hoursSinceCreated, string customerName)
    {
        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <div class='alert-box'>
                <h2>⏰ {title}</h2>
                <p>Ticket przypisany do Ciebie nie ma aktywności od <strong>{hoursSinceCreated} godzin</strong>.</p>
            </div>

            <p>Ten ticket wymaga Twojej uwagi. Klient <strong>{customerName}</strong> oczekuje na odpowiedź.</p>

            <div class='info-box'>
                <strong>📋 Szczegóły zgłoszenia:</strong>
                <ul>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Czas bez aktywności:</strong> {hoursSinceCreated}h</li>
                    <li><strong>Klient:</strong> {customerName}</li>
                </ul>
            </div>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Zajmij się Ticketem
                </a>
            </center>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)")
            .Replace("{headerIcon}", "⏰")
            .Replace("{headerTitle}", "Przypomnienie dla Agenta")
            .Replace("{headerSubtitle}", "Ticket wymaga Twojej uwagi")
            .Replace("{alertBackground}", "#fef3c7")
            .Replace("{alertBorderColor}", "#f59e0b")
            .Replace("{alertTitleColor}", "#92400e")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #f59e0b 0%, #d97706 100%)")
            .Replace("{bodyContent}", bodyContent);
    }

    public static string SlaBreached(string firstName, string title, Guid ticketId, int hoursOverdue, DateTime createdAt)
    {
        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <div class='alert-box'>
                <h2>🚨 {title}</h2>
                <p>Twój ticket <strong>przekroczył limit czasu SLA</strong> i został automatycznie eskalowany do priorytetu <strong>CRITICAL</strong>.</p>
            </div>

            <div class='info-box'>
                <strong>⚠️ Szczegóły eskalacji:</strong>
                <ul>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Czas przekroczenia:</strong> {hoursOverdue}h ({hoursOverdue / 24.0:F1} dni)</li>
                    <li><strong>Data utworzenia:</strong> {createdAt:dd.MM.yyyy HH:mm}</li>
                    <li><strong>Nowy priorytet:</strong> <span style='color: #dc2626; font-weight: 600;'>CRITICAL</span></li>
                </ul>
            </div>

            <p>Nasz zespół został powiadomiony o tym tickecie i nadano mu najwyższy priorytet. Skontaktujemy się z Tobą w najkrótszym możliwym czasie.</p>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Zobacz Ticket
                </a>
            </center>

            <p style='color: #6b7280; font-size: 14px; margin-top: 30px;'>
                Przepraszamy za opóźnienie. Twoja sprawa jest teraz naszym najwyższym priorytetem.
            </p>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #dc2626 0%, #991b1b 100%)")
            .Replace("{headerIcon}", "🚨")
            .Replace("{headerTitle}", "SLA BREACH - Eskalacja Krytyczna")
            .Replace("{headerSubtitle}", "Twoje zgłoszenie wymaga natychmiastowej uwagi")
            .Replace("{alertBackground}", "#fee2e2")
            .Replace("{alertBorderColor}", "#dc2626")
            .Replace("{alertTitleColor}", "#991b1b")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #dc2626 0%, #991b1b 100%)")
            .Replace("{bodyContent}", bodyContent);
    }

    public static string SlaBreachedAgent(string firstName, string title, Guid ticketId, int hoursOverdue, string customerName)
    {
        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <div class='alert-box'>
                <h2>🚨 URGENT: SLA Breach Alert</h2>
                <p>Ticket przypisany do Ciebie <strong>przekroczył SLA</strong> i został automatycznie eskalowany do <strong>CRITICAL</strong>.</p>
            </div>

            <div class='info-box'>
                <strong>⚠️ Szczegóły:</strong>
                <ul>
                    <li><strong>Ticket:</strong> {title}</li>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Czas overdue:</strong> {hoursOverdue}h ({hoursOverdue / 24.0:F1} dni)</li>
                    <li><strong>Klient:</strong> {customerName}</li>
                    <li><strong>Priorytet:</strong> <span style='color: #dc2626; font-weight: 600;'>CRITICAL</span></li>
                </ul>
            </div>

            <p style='background: #fee2e2; padding: 15px; border-radius: 6px; color: #991b1b; font-weight: 600; text-align: center;'>
                ⚠️ Wymagana natychmiastowa akcja!
            </p>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Akcja Natychmiast
                </a>
            </center>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #dc2626 0%, #991b1b 100%)")
            .Replace("{headerIcon}", "🚨")
            .Replace("{headerTitle}", "URGENT: SLA Breach")
            .Replace("{headerSubtitle}", "Natychmiastowa akcja wymagana")
            .Replace("{alertBackground}", "#fee2e2")
            .Replace("{alertBorderColor}", "#dc2626")
            .Replace("{alertTitleColor}", "#991b1b")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #dc2626 0%, #991b1b 100%)")
            .Replace("{bodyContent}", bodyContent);
    }

    public static string TicketCreated(string firstName, Guid ticketId, string title, string priority, string category)
    {
        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <p style='font-size: 16px;'>Dziękujemy za skontaktowanie się z naszym zespołem wsparcia. Twoje zgłoszenie zostało utworzone i jest już w trakcie przetwarzania.</p>

            <div class='info-box'>
                <strong>📋 Szczegóły zgłoszenia:</strong>
                <ul>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Tytuł:</strong> {title}</li>
                    <li><strong>Priorytet:</strong> {priority}</li>
                    <li><strong>Kategoria:</strong> {category}</li>
                    <li><strong>Status:</strong> Nowe</li>
                </ul>
            </div>

            <p>Nasz zespół przeanalizuje Twoje zgłoszenie i wkrótce się z Tobą skontaktuje. Możesz śledzić status swojego ticketu w panelu klienta.</p>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Zobacz Ticket
                </a>
            </center>

            <div class='divider'></div>

            <p style='color: #6b7280; font-size: 14px;'>
                <strong>💡 Wskazówka:</strong> Możesz dodać więcej informacji lub załączników do swojego ticketu w każdej chwili.
            </p>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #10b981 0%, #059669 100%)")
            .Replace("{headerIcon}", "✅")
            .Replace("{headerTitle}", "Ticket Utworzony")
            .Replace("{headerSubtitle}", "Twoje zgłoszenie zostało zarejestrowane")
            .Replace("{alertBackground}", "#d1fae5")
            .Replace("{alertBorderColor}", "#10b981")
            .Replace("{alertTitleColor}", "#065f46")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)")
            .Replace("{bodyContent}", bodyContent);
    }

    public static string TicketAssigned(string firstName, Guid ticketId, string title, string agentName)
    {
        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <div class='alert-box'>
                <h2>👤 {title}</h2>
                <p>Twój ticket został przypisany do agenta <strong>{agentName}</strong>.</p>
            </div>

            <p>Dobra wiadomość! Agent <strong>{agentName}</strong> zajmie się Twoim zgłoszeniem i wkrótce się z Tobą skontaktuje.</p>

            <div class='info-box'>
                <strong>📋 Szczegóły:</strong>
                <ul>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Przypisany agent:</strong> {agentName}</li>
                    <li><strong>Status:</strong> Przypisany</li>
                </ul>
            </div>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Zobacz Ticket
                </a>
            </center>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%)")
            .Replace("{headerIcon}", "👤")
            .Replace("{headerTitle}", "Ticket Przypisany")
            .Replace("{headerSubtitle}", "Agent zajmie się Twoim zgłoszeniem")
            .Replace("{alertBackground}", "#ede9fe")
            .Replace("{alertBorderColor}", "#8b5cf6")
            .Replace("{alertTitleColor}", "#5b21b6")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #8b5cf6 0%, #7c3aed 100%)")
            .Replace("{bodyContent}", bodyContent);
    }

    public static string TicketAssignedAgent(string firstName, Guid ticketId, string title, string customerName, string priority)
    {
        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <div class='alert-box'>
                <h2>📥 Nowy Ticket Przypisany</h2>
                <p>Przypisano Ci nowe zgłoszenie od klienta <strong>{customerName}</strong>.</p>
            </div>

            <div class='info-box'>
                <strong>📋 Szczegóły zgłoszenia:</strong>
                <ul>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Tytuł:</strong> {title}</li>
                    <li><strong>Klient:</strong> {customerName}</li>
                    <li><strong>Priorytet:</strong> {priority}</li>
                </ul>
            </div>

            <p>Prosimy o przejrzenie zgłoszenia i odpowiedź na nie w odpowiednim czasie zgodnie z priorytetem.</p>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Zobacz Ticket
                </a>
            </center>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)")
            .Replace("{headerIcon}", "📥")
            .Replace("{headerTitle}", "Nowy Ticket dla Ciebie")
            .Replace("{headerSubtitle}", "Zgłoszenie oczekuje na Twoją odpowiedź")
            .Replace("{alertBackground}", "#dbeafe")
            .Replace("{alertBorderColor}", "#3b82f6")
            .Replace("{alertTitleColor}", "#1e40af")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)")
            .Replace("{bodyContent}", bodyContent);
    }

    public static string CommentAdded(string firstName, Guid ticketId, string title, string agentName, string commentPreview)
    {
        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <div class='alert-box'>
                <h2>💬 Nowa Odpowiedź</h2>
                <p>Agent <strong>{agentName}</strong> odpowiedział na Twój ticket.</p>
            </div>

            <div class='info-box'>
                <strong>📋 Szczegóły:</strong>
                <ul>
                    <li><strong>Ticket:</strong> {title}</li>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Autor:</strong> {agentName}</li>
                </ul>
            </div>

            <div style='background: #f9fafb; border-radius: 8px; padding: 20px; margin: 25px 0; border: 1px solid #e5e7eb;'>
                <p style='color: #6b7280; font-size: 14px; margin-bottom: 8px;'><strong>Podgląd komentarza:</strong></p>
                <p style='color: #1f2937; font-style: italic;'>{commentPreview}</p>
            </div>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Zobacz Pełną Odpowiedź
                </a>
            </center>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #06b6d4 0%, #0891b2 100%)")
            .Replace("{headerIcon}", "💬")
            .Replace("{headerTitle}", "Nowa Odpowiedź")
            .Replace("{headerSubtitle}", "Agent odpowiedział na Twoje zgłoszenie")
            .Replace("{alertBackground}", "#cffafe")
            .Replace("{alertBorderColor}", "#06b6d4")
            .Replace("{alertTitleColor}", "#155e75")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #06b6d4 0%, #0891b2 100%)")
            .Replace("{bodyContent}", bodyContent);
    }

    public static string TicketStatusChanged(string firstName, Guid ticketId, string title, string oldStatus, string newStatus)
    {
        var statusIcon = newStatus.ToLower() switch
        {
            "resolved" => "✅",
            "closed" => "🔒",
            "in progress" => "⚙️",
            "on hold" => "⏸️",
            _ => "📝"
        };

        var statusMessage = newStatus.ToLower() switch
        {
            "resolved" => "Dobra wiadomość! Twój ticket został rozwiązany.",
            "closed" => "Ticket został zamknięty. Dziękujemy za skorzystanie z helpdesku!",
            "in progress" => "Świetnie! Pracujemy nad rozwiązaniem Twojego problemu.",
            "on hold" => "Ticket został tymczasowo wstrzymany.",
            _ => "Status Twojego ticketu został zaktualizowany."
        };

        var bodyContent = $@"
            <p class='greeting'>Witaj <strong>{firstName}</strong>!</p>

            <div class='alert-box'>
                <h2>{statusIcon} {title}</h2>
                <p>{statusMessage}</p>
            </div>

            <div class='info-box'>
                <strong>📋 Szczegóły zmiany:</strong>
                <ul>
                    <li><strong>Ticket ID:</strong> #{ticketId}</li>
                    <li><strong>Poprzedni status:</strong> {oldStatus}</li>
                    <li><strong>Nowy status:</strong> <span style='color: #059669; font-weight: 600;'>{newStatus}</span></li>
                </ul>
            </div>

            <center>
                <a href='http://localhost:5173/tickets/{ticketId}' class='cta-button'>
                    Zobacz Ticket
                </a>
            </center>";

        return BaseTemplate
            .Replace("{headerGradient}", "linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)")
            .Replace("{headerIcon}", statusIcon)
            .Replace("{headerTitle}", "Status Zmieniony")
            .Replace("{headerSubtitle}", "Aktualizacja statusu zgłoszenia")
            .Replace("{alertBackground}", "#e0e7ff")
            .Replace("{alertBorderColor}", "#6366f1")
            .Replace("{alertTitleColor}", "#3730a3")
            .Replace("{buttonBackground}", "linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)")
            .Replace("{bodyContent}", bodyContent);
    }
}
