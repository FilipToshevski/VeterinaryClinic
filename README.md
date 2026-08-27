# 🐾 VeterinaryClinic

A full-stack **ASP.NET Core MVC** web application for managing a veterinary clinic — built as a
school/personal project to practice EF Core, ASP.NET Identity, role-based authorization, and
Razor-based server-rendered UI.

Pet owners can register, manage their profile, and track their pets' vaccine history. Clinic
admins get a dedicated area to manage owners, pets, and vaccines across the whole clinic.

> This project was originally built in early 2025 and has since gone through a cleanup pass —
> removing hardcoded credentials, fixing a couple of bugs, and tidying up dead code — to bring it
> up to a standard worth sharing.

---

## Features

**For pet owners (registered users)**
- Register and log in via ASP.NET Core Identity
- View and edit personal profile details
- View all owned pets and each pet's vaccination history
- Self-service password reset flow

**For clinic admins**
- Dedicated `/Admin` dashboard
- **Owners**: create, edit, and delete owner accounts
- **Pets**: create, edit, and delete pets; search by owner, pet name, or animal type
- **Vaccines**: create and delete vaccine types; assign or remove vaccines from a specific pet

## Tech stack

| Layer            | Technology |
|-------------------|------------|
| Framework          | ASP.NET Core MVC (.NET 8) |
| Data access        | Entity Framework Core |
| Database           | SQL Server / LocalDB |
| Auth               | ASP.NET Core Identity, role-based (`Admin`, `User`) |
| Views              | Razor views + tag helpers |
| Validation         | DataAnnotations, incl. a custom `AgeRangeAttribute` |

## Data model

`Owner` extends `IdentityUser` and has many `Pets`. Each `Pets` can have many `Vaccine`s through
a `PetVaccine` join entity, which also stores `DateAdministered` for that specific pet/vaccine
pairing — a proper many-to-many relationship **with payload**, not just a bare join table.

```
Owner (1) ── (many) Pets (many) ── (many) Vaccine
                        via PetVaccine (PetId, VaccineId, DateAdministered)
```

## Project structure

```
Controllers/    MVC controllers (Account, Admin, Home, Privacy)
Models/         EF Core entities (Owner, Pets, Vaccine, PetVaccine)
ViewModels/     View-specific models, kept separate from entities
Views/          Razor views, organized by controller
Data/           VeterinaryClinicDb — EF Core DbContext (extends IdentityDbContext)
Attributes/     Custom validation attributes (AgeRangeAttribute)
Extentions/     Shared extension methods (e.g. DateTime.CalculateAge())
Migrations/     EF Core migrations
```

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (comes with Visual Studio) or any reachable SQL Server instance

### 1. Clone and restore

```bash
git clone https://github.com/FilipToshevski/VeterinaryClinic.git
cd VeterinaryClinic
dotnet restore
```

### 2. Configure the database connection

`appsettings.json` defaults to LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=ClinicDb;Trusted_Connection=True;
```

Update this if you're pointing at a different SQL Server instance.

### 3. Configure the seeded admin account

No admin credentials are hardcoded in source. On first run, the app seeds `Admin`/`User` roles,
and seeds one admin account **only if credentials are configured**. Set them locally with
[user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets init
dotnet user-secrets set "AdminSeed:Email" "admin@example.com"
dotnet user-secrets set "AdminSeed:Password" "ChooseA-Strong-Password1!"
```

In any non-development environment, set these via environment variables instead
(`AdminSeed__Email`, `AdminSeed__Password`) or your hosting platform's secret store. If unset,
the app just skips seeding and logs a warning — no crash, no default password.

### 4. Apply migrations

```bash
dotnet ef database update
```

### 5. Run

```bash
dotnet run
```

Open the URL shown in the console, register a normal account to try the owner-facing flow, or
log in with your configured admin account to reach `/Admin`.

## Known limitations / roadmap

- Password reset currently surfaces the reset link via `TempData` for local testing rather than
  emailing it — a real provider (SendGrid, SMTP, etc.) is needed before any real deployment.
- Animal types are a hardcoded list in `AdminController` rather than a database-backed lookup
  table.
- No automated tests yet.

## License

This project doesn't currently specify a license. If you'd like others to be able to use or
build on this code, consider adding one (e.g. [MIT](https://choosealicense.com/licenses/mit/)).
