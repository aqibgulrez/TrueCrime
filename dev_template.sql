-- dev_template.sql
-- Minimal schema and seed data for local development (sanitized)

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email" TEXT NOT NULL,
    "FullName" TEXT,
    "Role" TEXT NOT NULL DEFAULT 'User',
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "LatestStoryId" UUID NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT now(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE TABLE IF NOT EXISTS "UserStories" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Content" TEXT NOT NULL,
    "TokenCost" INTEGER DEFAULT 0,
    "ModerationStatus" TEXT DEFAULT 'Unchecked',
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE TABLE IF NOT EXISTS "BillingPlans" (
    "PlanId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" TEXT NOT NULL,
    "Cadence" TEXT NOT NULL,
    "StripePriceId" TEXT,
    "Price" NUMERIC(10,2),
    "Currency" TEXT DEFAULT 'USD',
    "TrialDays" INTEGER DEFAULT 0,
    "IsActive" BOOLEAN DEFAULT TRUE
);

-- Seed a developer plan
INSERT INTO "BillingPlans" ("Name","Cadence","Price","Currency","IsActive") VALUES ('Dev Monthly','monthly',0.00,'USD',TRUE) ON CONFLICT DO NOTHING;
