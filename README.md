# FirstWebApplication1 – IS-202 Programmeringsprosjekt (Høst 2025)

FirstWebApplication1 is an ASP.NET Core MVC web application developed as part of IS-202 Programmeringsprosjekt (Høst 2025) at the University of Agder.

The application lets authenticated users register aviation-related obstacles through a two-step form (choose type → register details with map input) and view submitted entries in a sortable/filterable list. Pilots can also open a Leaflet-based map that shows both pending and approved submissions for situational awareness.

The solution can run fully in Docker (ASP.NET Core 9 container + MariaDB) or directly from `dotnet`/Visual Studio with a running database. Docker Compose files are provided for the simplest setup.

## Repository structure
- `FirstWebApplication1/` – ASP.NET Core MVC app (controllers, models, Razor views, static assets).
- `FirstWebApplication1.Tests/` – Testprosjekt for å legge til enhets-/integrasjonstester.
- `docs/` – Utfyllende dokumentasjon (arkitektur, sikkerhet, testing, mobil).
- Docker Compose-filer – Starter MariaDB og app-containere i utvikling.

## Teknologi og nøkkelfunksjoner
- ASP.NET Core 9 MVC med Identity og EF Core (Pomelo MariaDB-driver)
- Tailwind CSS via CDN og Leaflet-kart (viser godkjente **og** ventende hindre)
- Rollebasert tilgang (`Admin`, `Caseworker`, `Pilot`) med rate limiting på `ObstacleController`
- HTTPS/HSTS utenfor utvikling, standard antiforgery på POST-aksjoner, Razor-encoding mot XSS og EF Core-parameterisering mot SQL-injection; ingen ekstra sikkerhetshoder er satt i koden

# Technologies and tools used
- JavaScript
- C#
- JSON
- Markdown
- MariaDB
- Docker
- Nuget

# How the project is run

You can start the system with Docker Compose **or** run it directly from `dotnet run` once MariaDB is available.

1. Clone the repository
   - `git clone https://github.com/KristianEspevikUIA/KartVerketProsjektGR20.git`
   - `cd KartverketProsjektGR20`

2. Restore packages
   - `dotnet restore`

3. Start the services (Docker)
   - Ensure Docker is running and the external network `appnet` exists (`docker network create appnet` if missing)
   - `docker compose up --build`
   - The web app listens on http://localhost:5010 and connects to the bundled MariaDB instance

4. Alternative local run (without Docker for the app)
   - Start MariaDB separately and set `ConnectionStrings__DefaultConnection`
   - Run `dotnet ef database update` (applies migrations) then `dotnet run --project FirstWebApplication1`
   - Open http://localhost:5010 (or the port shown in the output)

# Project Setup
## Docker Background Services

The project uses:

- A MariaDB container for storing obstacle data

- An ASP.NET Core 9 container for running the application (automatically launched by Visual Studio or Docker Compose)

## Admin account setup

Roles (`Admin`, `Pilot`, `Caseworker`) are seeded on startup. An admin user is provisioned only when the following configuration values are present:

- `Admin:Email` – the admin username (default suggestion: `admin@kartverket.no`)
- `Admin:Password` – the initial admin password (example development value: `Admin123`)

If these values are missing, the application logs a warning and no admin user is created. Only the configured admin email can sign up as an administrator; the public registration form exposes Pilot and Caseworker roles only.

# Running the Application

- Start Docker Desktop (if using containers)
- Open the solution or run `dotnet run --project FirstWebApplication1`
- The default exposed port (for local development) is: http://localhost:5010

## How the system works (high level)
- Users register/login via ASP.NET Identity. Only preconfigured emails can become Admin; other users choose Pilot/Caseworker.
- Obstacle flow: select obstacle type → fill out form (map + metadata) → submission saves as `Pending` → confirmation view.
- Review: Caseworker/Admin filter obstacles, update status (Approved/Declined/Pending) and see modification metadata.
- Visibility: Approved and pending obstacles are exposed as JSON to the pilot map, while the list view is role-gated (Pilot/Caseworker/Admin) with filtering by status, date range, height, type, and organization.

## Documentation
- Systemarkitektur: `docs/architecture.md`
- Sikkerhet (authn/autorisasjon, rate limiting): `docs/security.md`
- Testing (plan, scenarier, logg): `docs/testing.md`
- Mobil og responsivitet: `docs/mobile.md` (inkl. skjermbilder)

# Midlertidig håndtering av passord i repoet (kun for sensur)
- Vi har **bevisst sjekket inn databasepassord og admin-passord** i Git for å forenkle oppsettet under sensur.
- Dette er **ikke en anbefalt praksis** og bryter med våre egne retningslinjer om å bruke miljøvariabler/User Secrets for hemmeligheter.
- Etter at prosjektet er ferdig vurdert vil passordene roteres, flyttes til secrets og slettes fra historikken for å gjenopprette sikkerhetsnivået.

# Project Features

The application includes:

- A clean obstacle registration form capturing height, location, coordinates, category, and metadata
- A result/confirmation view that shows the submitted information
- A table overview of reported obstacles with filters for status, dates, height, type, organization, and text search
- A Leaflet-based interactive map for displaying positions (point or drawn line) of approved and pending obstacles
- Feet/meters conversion support based on user role
- A simple and extendable architecture for further development throughout the IS-202 course

## Pilot-facing obstacle overview

Pilots have two dedicated entry points for situational awareness:

- **Pilot map (`/Pilot/Map`)** – loads Leaflet with approved and pending obstacles from `PilotController.GetApprovedObstacles`, rendering both point markers and optional line geometry. A floating button links directly to the obstacle submission flow so pilots can report new findings.

- **Obstacle list (`/Obstacle/List`)** – role-gated for Pilot, Caseworker, and Admin, exposing filtering by status, type, text search, date range, height, and organization. Each row links to detail and edit actions, giving pilots a clear, filterable overview of all stored obstacles.

This is a practical programming assignment focused on:

- ASP.NET Core MVC development

- Docker and containerized databases

- Form handling

- Razor views

- Basic JavaScript map integration

# Project Purpose and Context
The project was developed for IS-202 Programmeringsprosjekt, where students are tasked with building a functioning software solution based on given requirements. Our group implemented an obstacle reporting system inspired by processes used by Kartverket and Norsk Luftambulanse. The application supports creating new obstacle reports, managing them, and displaying the data in dynamic interfaces such as tables and a map view.

All features were developed collaboratively by the group, including form validation, data handling, UI adjustments, status history, and map integration.

## 📄 Dokumentasjon

- [Systemarkitektur](docs/architecture.md)
- [Mobiltilpasning](docs/mobile.md)
- [Testing og testresultater](docs/testing.md)

# Team Members

This project was developed by Group 20:

Nicolai Stephansen

Brage Kristoffersen

Endi Muriqi

Kristian Espevik

Rune Kvame

Victor Ziadpour
