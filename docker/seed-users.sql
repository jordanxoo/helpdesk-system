-- ========================================
-- SEED DATA - Użytkownicy testowi
-- Wykonaj ten skrypt RĘCZNIE po pierwszym uruchomieniu
-- ========================================

-- HELPDESK_AUTH - AspNetUsers (Identity)
\c helpdesk_auth;

-- Sprawdź czy tabela istnieje
DO $$
BEGIN
    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'AspNetRoles') THEN
        
        -- Dodaj role
        INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
        VALUES 
            ('customer-role-id', 'Customer', 'CUSTOMER', 'role-stamp-1'),
            ('agent-role-id', 'Agent', 'AGENT', 'role-stamp-2'),
            ('admin-role-id', 'Administrator', 'ADMINISTRATOR', 'role-stamp-3')
        ON CONFLICT DO NOTHING;

        -- Dodaj użytkowników (hashe dla: Customer123!, Agent123!, Admin123!)
        INSERT INTO "AspNetUsers" (
            "Id", 
            "UserName", 
            "NormalizedUserName", 
            "Email", 
            "NormalizedEmail", 
            "EmailConfirmed",
            "PasswordHash",
            "SecurityStamp",
            "ConcurrencyStamp",
            "PhoneNumberConfirmed",
            "TwoFactorEnabled",
            "LockoutEnabled",
            "AccessFailedCount",
            "FirstName",
            "LastName",
            "CreatedAt"
        )
        VALUES 
            (
                'customer-user-id-001',
                'customer@test.com',
                'CUSTOMER@TEST.COM',
                'customer@test.com',
                'CUSTOMER@TEST.COM',
                true,
                'AQAAAAIAAYagAAAAEGq7qK8xN7fJNj3h3LNJ9k7jGvBH8YKN2xB4Q+R5fK9H1zM3pL8wE6vD9yF7xA==', -- Customer123!
                'security-stamp-1',
                'concurrency-stamp-1',
                false,
                false,
                true,
                0,
                'Jan',
                'Kowalski',
                NOW()
            ),
            (
                'agent-user-id-001',
                'agent@test.com',
                'AGENT@TEST.COM',
                'agent@test.com',
                'AGENT@TEST.COM',
                true,
                'AQAAAAIAAYagAAAAEGq7qK8xN7fJNj3h3LNJ9k7jGvBH8YKN2xB4Q+R5fK9H1zM3pL8wE6vD9yF7xA==', -- Agent123!
                'security-stamp-2',
                'concurrency-stamp-2',
                false,
                false,
                true,
                0,
                'Anna',
                'Nowak',
                NOW()
            ),
            (
                'admin-user-id-001',
                'admin@test.com',
                'ADMIN@TEST.COM',
                'admin@test.com',
                'ADMIN@TEST.COM',
                true,
                'AQAAAAIAAYagAAAAEGq7qK8xN7fJNj3h3LNJ9k7jGvBH8YKN2xB4Q+R5fK9H1zM3pL8wE6vD9yF7xA==', -- Admin123!
                'security-stamp-3',
                'concurrency-stamp-3',
                false,
                false,
                true,
                0,
                'Piotr',
                'Wiśniewski',
                NOW()
            )
        ON CONFLICT DO NOTHING;

        -- Przypisz role do użytkowników
        INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
        VALUES 
            ('customer-user-id-001', 'customer-role-id'),
            ('agent-user-id-001', 'agent-role-id'),
            ('admin-user-id-001', 'admin-role-id')
        ON CONFLICT DO NOTHING;

    END IF;
END $$;

-- HELPDESK_USERS - Users (Profile data)
\c helpdesk_users;

DO $$
BEGIN
    IF EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Users') THEN
        
        INSERT INTO "Users" (
            "Id",
            "Email",
            "FirstName",
            "LastName",
            "PhoneNumber",
            "Role",
            "IsActive",
            "CreatedAt",
            "UpdatedAt"
        )
        VALUES 
            (
                'customer-user-id-001',
                'customer@test.com',
                'Jan',
                'Kowalski',
                '+48 123 456 789',
                0, -- Customer = 0
                true,
                NOW(),
                NOW()
            ),
            (
                'agent-user-id-001',
                'agent@test.com',
                'Anna',
                'Nowak',
                '+48 987 654 321',
                1, -- Agent = 1
                true,
                NOW(),
                NOW()
            ),
            (
                'admin-user-id-001',
                'admin@test.com',
                'Piotr',
                'Wiśniewski',
                '+48 555 666 777',
                2, -- Administrator = 2
                true,
                NOW(),
                NOW()
            )
        ON CONFLICT DO NOTHING;

    END IF;
END $$;