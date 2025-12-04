-- ========================================
-- SEED DATA - Tickety testowe
-- Wykonaj ten skrypt RĘCZNIE po pierwszym uruchomieniu
-- ========================================

-- UUID użytkowników z bazy auth (sprawdź aktualne wartości w helpdesk_auth.users)
-- Customer: c6b612a9-c9e0-4ffb-af3d-fe37ca2da0f7 (cdustomer@helpdesk.com)
-- Agent: f10db4fb-4a77-4e79-94cd-64f122de34e4 (agent@test.com)
-- Admin: 06343827-0b21-4acf-bb16-d3ed06099282 (admin@test.com)

\c helpdesk_tickets;

DO $$
DECLARE
    customer_id UUID := 'c6b612a9-c9e0-4ffb-af3d-fe37ca2da0f7';
    agent_id UUID := 'f10db4fb-4a77-4e79-94cd-64f122de34e4';
BEGIN
    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'tickets') THEN
        
        -- Dodaj przykładowe tickety
        INSERT INTO "tickets" (
            "id",
            "title",
            "description",
            "status",
            "priority",
            "category",
            "customer_id",
            "assigned_agent_id",
            "organization_id",
            "sla_id",
            "created_at",
            "updated_at",
            "resolved_at"
        )
        VALUES 
            -- Ticket 1: Nowy, nieprzypisany
            (
                'a1111111-1111-1111-1111-111111111111',
                'Nie mogę zalogować się do systemu',
                'Od wczoraj próbuję się zalogować do systemu firmowego, ale ciągle wyświetla mi błąd "Invalid credentials". Hasło resetowałem już 3 razy i nadal nie działa.',
                'New',
                'High',
                'Account',
                customer_id,
                NULL,
                NULL,
                NULL,
                NOW() - INTERVAL '2 hours',
                NULL,
                NULL
            ),
            -- Ticket 2: Otwarty, przypisany do agenta
            (
                'a2222222-2222-2222-2222-222222222222',
                'Komputer nie uruchamia się',
                'Mój komputer firmowy Dell Latitude nie chce się uruchomić. Po naciśnięciu przycisku power świeci się tylko dioda LED, ale ekran pozostaje czarny. Potrzebuję pilnie komputera do pracy.',
                'Open',
                'Critical',
                'Hardware',
                customer_id,
                agent_id,
                NULL,
                NULL,
                NOW() - INTERVAL '1 day',
                NOW() - INTERVAL '23 hours',
                NULL
            ),
            -- Ticket 3: W trakcie realizacji
            (
                'a3333333-3333-3333-3333-333333333333',
                'Prośba o instalację Visual Studio',
                'Potrzebuję zainstalować Visual Studio 2022 Professional na moim komputerze służbowym. Numer inwentarzowy: INV-2024-0542. Proszę o wersję z rozszerzeniami do .NET i C++.',
                'InProgress',
                'Medium',
                'Software',
                customer_id,
                agent_id,
                NULL,
                NULL,
                NOW() - INTERVAL '3 days',
                NOW() - INTERVAL '2 days',
                NULL
            ),
            -- Ticket 4: Oczekujący na odpowiedź klienta
            (
                'a4444444-4444-4444-4444-444444444444',
                'Wolne połączenie internetowe',
                'Internet w moim biurze (pokój 205) jest bardzo wolny. Strony ładują się po 30 sekund, a videokonferencje się rwą. Problem występuje od poniedziałku.',
                'Pending',
                'Medium',
                'Network',
                customer_id,
                agent_id,
                NULL,
                NULL,
                NOW() - INTERVAL '5 days',
                NOW() - INTERVAL '4 days',
                NULL
            ),
            -- Ticket 5: Rozwiązany
            (
                'a5555555-5555-5555-5555-555555555555',
                'Reset hasła do poczty',
                'Zapomniałem hasła do swojej skrzynki pocztowej firmowej. Proszę o reset hasła.',
                'Resolved',
                'Low',
                'Account',
                customer_id,
                agent_id,
                NULL,
                NULL,
                NOW() - INTERVAL '7 days',
                NOW() - INTERVAL '6 days',
                NOW() - INTERVAL '6 days'
            ),
            -- Ticket 6: Zamknięty
            (
                'a6666666-6666-6666-6666-666666666666',
                'Wymiana klawiatury',
                'Klawiatura przy moim stanowisku pracy ma zepsute klawisze: spacja, enter i backspace. Proszę o wymianę na nową.',
                'Closed',
                'Low',
                'Hardware',
                customer_id,
                agent_id,
                NULL,
                NULL,
                NOW() - INTERVAL '14 days',
                NOW() - INTERVAL '10 days',
                NOW() - INTERVAL '10 days'
            ),
            -- Ticket 7: Nowy, krytyczny problem bezpieczeństwa
            (
                'a7777777-7777-7777-7777-777777777777',
                'Podejrzany email z załącznikiem',
                'Otrzymałem email od nieznanego nadawcy z załącznikiem .exe. Email wyglądał jakby był od naszego CEO. Nie otworzyłem załącznika. Proszę o sprawdzenie czy to phishing.',
                'New',
                'Critical',
                'Security',
                customer_id,
                NULL,
                NULL,
                NULL,
                NOW() - INTERVAL '30 minutes',
                NULL,
                NULL
            ),
            -- Ticket 8: Otwarty, problem z siecią
            (
                'a8888888-8888-8888-8888-888888888888',
                'Brak dostępu do drukarki sieciowej',
                'Nie mogę drukować na drukarce HP LaserJet w pokoju 301. Drukarka jest widoczna w sieci, ale przy próbie drukowania pojawia się błąd "Printer offline".',
                'Open',
                'Medium',
                'Network',
                customer_id,
                agent_id,
                NULL,
                NULL,
                NOW() - INTERVAL '6 hours',
                NOW() - INTERVAL '5 hours',
                NULL
            ),
            -- Ticket 9: W realizacji, problem z oprogramowaniem
            (
                'a9999999-9999-9999-9999-999999999999',
                'Excel zawiesza się przy dużych plikach',
                'Microsoft Excel 2021 zawiesza się gdy próbuję otworzyć pliki większe niż 50MB. Komputer ma 16GB RAM więc nie powinno być problemu z pamięcią. Proszę o pomoc.',
                'InProgress',
                'High',
                'Software',
                customer_id,
                agent_id,
                NULL,
                NULL,
                NOW() - INTERVAL '2 days',
                NOW() - INTERVAL '1 day',
                NULL
            ),
            -- Ticket 10: Nowy, inne
            (
                'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                'Prośba o dodatkowy monitor',
                'Czy istnieje możliwość otrzymania drugiego monitora do mojego stanowiska pracy? Praca na dwóch ekranach znacznie zwiększyłaby moją produktywność.',
                'New',
                'Low',
                'Other',
                customer_id,
                NULL,
                NULL,
                NULL,
                NOW() - INTERVAL '1 hour',
                NULL,
                NULL
            )
        ON CONFLICT (id) DO NOTHING;

        -- Dodaj komentarze do ticketów
        INSERT INTO "ticket_comments" (
            "id",
            "ticket_id",
            "user_id",
            "content",
            "is_internal",
            "created_at"
        )
        VALUES
            -- Komentarze do ticketa 2 (Komputer nie uruchamia się)
            (
                'c1111111-1111-1111-1111-111111111111',
                'a2222222-2222-2222-2222-222222222222',
                agent_id,
                'Dzień dobry, przyjąłem zgłoszenie. Czy mógłby Pan sprawdzić czy kabel zasilający jest prawidłowo podłączony?',
                false,
                NOW() - INTERVAL '23 hours'
            ),
            (
                'c1111111-1111-1111-1111-111111111112',
                'a2222222-2222-2222-2222-222222222222',
                customer_id,
                'Sprawdziłem - kabel jest dobrze podłączony. Próbowałem też inny kabel i nadal to samo.',
                false,
                NOW() - INTERVAL '22 hours'
            ),
            (
                'c1111111-1111-1111-1111-111111111113',
                'a2222222-2222-2222-2222-222222222222',
                agent_id,
                'Notatka wewnętrzna: Prawdopodobnie uszkodzony zasilacz lub płyta główna. Należy zamówić wymianę.',
                true,
                NOW() - INTERVAL '21 hours'
            ),
            -- Komentarze do ticketa 3 (Visual Studio)
            (
                'c2222222-2222-2222-2222-222222222221',
                'a3333333-3333-3333-3333-333333333333',
                agent_id,
                'Instalacja Visual Studio 2022 Professional została zaplanowana na jutro. Proszę o pozostawienie komputera włączonego po godzinie 18:00.',
                false,
                NOW() - INTERVAL '2 days'
            ),
            -- Komentarze do ticketa 4 (Wolny internet)
            (
                'c3333333-3333-3333-3333-333333333331',
                'a4444444-4444-4444-4444-444444444444',
                agent_id,
                'Przeprowadziłem testy sieci. Proszę o informację czy problem nadal występuje po ponownym uruchomieniu routera w pokoju.',
                false,
                NOW() - INTERVAL '4 days'
            ),
            -- Komentarze do ticketa 5 (Reset hasła - rozwiązany)
            (
                'c4444444-4444-4444-4444-444444444441',
                'a5555555-5555-5555-5555-555555555555',
                agent_id,
                'Hasło zostało zresetowane. Nowe tymczasowe hasło wysłałem na Pana numer telefonu służbowego. Proszę o zmianę hasła przy pierwszym logowaniu.',
                false,
                NOW() - INTERVAL '6 days'
            ),
            (
                'c4444444-4444-4444-4444-444444444442',
                'a5555555-5555-5555-5555-555555555555',
                customer_id,
                'Dziękuję, udało się zalogować. Hasło zmienione.',
                false,
                NOW() - INTERVAL '6 days' + INTERVAL '1 hour'
            )
        ON CONFLICT (id) DO NOTHING;

        RAISE NOTICE 'Tickety i komentarze zostały dodane pomyślnie!';
        
    ELSE
        RAISE NOTICE 'Tabela tickets nie istnieje. Uruchom najpierw migracje.';
    END IF;
END $$;

-- Pokaż statystyki
SELECT 
    'Tickets' as table_name, 
    COUNT(*) as count 
FROM tickets
UNION ALL
SELECT 
    'Comments' as table_name, 
    COUNT(*) as count 
FROM ticket_comments;
