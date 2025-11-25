# FirstWebApplication1 – IS-202 Programmeringsprosjekt (Høst 2025)

FirstWebApplication1 is an ASP.NET Core MVC web application developed as part of
IS-202 Programmeringsprosjekt (Høst 2025) at the University of Agder.

The application allows users to register aviation-related obstacles through a structured form and view submitted entries in a clear overview table.
It integrates a simple interactive map for visualising registered coordinates and provides a user-friendly submission workflow designed for further extension.

The project is developed and run in Visual Studio using Docker Compose for the database container and for managing environment variables. Docker runs in the background while Visual Studio launches the web application automatically.

# Technologies and tools used 
- JavaScript
- C#
- JSON
- Markdown
- MariaDB
- Docker
- Nuget



# How the project is run:

Development and execution are done directly from Visual Studio:

1. Clone the repository.
- git clone https://github.com/KristianEspevikUIA/KartverketProsjektGR20.git

2. Navigate to the project directory
- cd KartverketProsjektGR20

3. Install the dependecies
- docker build -t KristianEspevikUIA/KartVerketProsjektGR20

4. using nuget
- dotnet restore 


USAGE -----
5. Open the solution file:
FirstWebApplication1.sln

6. Make sure Docker Desktop is running in the background

7. In Visual Studio, select the Docker Compose target
(green play-button dropdown)

7. Press ▶ Docker Compose to start the app

Visual Studio builds both the Docker services and the ASP.NET Core application.
You do not need to manually run Docker commands.

# Project Setup
## Docker Background Services

The project uses:

- A MariaDB container for storing obstacle data

- An ASP.NET Core 9 container for running the application
(automatically launched by Visual Studio)

## Admin account setup

An admin user is provisioned automatically during startup when the following configuration values are present:

- `Admin:Email` – the admin username (default: `admin@kartverket.no`)
- `Admin:Password` – the initial admin password (default development value: `Admin123`)

If these values are missing, the application logs a warning and no admin user is created. Only the configured admin email can sign up as an administrator; the public registration form exposes Pilot and Caseworker roles only.

# Running the Application

1. Start Docker Desktop

2. Open the solution

3. Select Docker Compose from the Visual Studio run dropdown
(usually located next to the green start button)

4. Press Run

Visual Studio will:

- Start the MariaDB container

- Build and run the ASP.NET Core application

- Open the website in your browser

The default exposed port (for local development) is: http://localhost:5010

# Project Features

The application includes:

- A clean obstacle registration form
capturing height, location, coordinates, category, and metadata

- A result/confirmation view that shows the submitted information

- A table overview of reported obstacles

- A Leaflet-based interactive map for displaying positions

- Feet/meters conversion support based on user role

- A simple and extendable architecture for further development
throughout the IS-202 course

This is a practical programming assignment focused on:

- ASP.NET Core MVC development

- Docker and containerized databases

- Form handling

- Razor views

- Basic JavaScript map integration

# Project Purpose and Context
The project is developed for IS-202 Programmeringsprosjekt, where students are tasked with building a functioning software solution based on given requirements. Our group implemented an obstacle reporting system inspired by processes used by Kartverket and Norsk Luftambulanse. The application supports creating new obstacle reports, managing them, and displaying the data in dynamic interfaces such as tables and a map view.

All features were developed collaboratively by the group, including form validation, data handling, UI adjustments, status history, and map integration.

# Team Members

This project was developed by Group 20:

Nicolai Stephansen

Brage Kristoffersen

Endi Muriqi

Kristian Espevik

Rune Kvame

Victor Ziadpour
