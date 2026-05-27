# 🎬 Movie Booking Management

A full-stack cinema booking application allowing users to view movies, showtimes, select seats, and book tickets, along with administrative capabilities for managing movie catalogs. The project uses an **ASP.NET Core Web API (.NET 10)** backend and an **Angular 18** frontend.

---

## 🏗️ Project Architecture & Tech Stack

This project is divided into two primary directories:

### 1. Backend (`/Backend`)
* **Framework:** ASP.NET Core Web API (.NET 10)
* **Database ORM:** Entity Framework Core 10.0.8 (SQL Server LocalDB)
* **Security & Auth:** JWT Bearer Authentication & Role-Based Authorization (`Admin` and `Customer` roles)
* **Key Features:**
  * Global exception handling middleware (`GlobalExceptionMiddleware`).
  * Automatic Database Creation (`CineBookingDb`) and schema check updates.
  * Seeds initial data including movies, showtimes, and an admin user.
  * API Endpoints:
    * `GET /api/shows` - Retrieve all movie showtimes (open access)
    * `POST /api/shows/movie` - Add a new movie & showtimes (requires `Admin` role)
    * `DELETE /api/shows/movie/{id}` - Delete a movie (requires `Admin` role)
    * `GET /api/bookings/seats/{showtimeId}` - Retrieve seat reservations for a showtime
    * `POST /api/bookings` - Reserve seats (requires authenticated user)

### 2. Frontend (`/cine-booking`)
* **Framework:** Angular 18
* **Styles:** Responsive cinema seat layout and modern CSS design
* **State & HTTP:** RxJS, custom Angular HTTP interceptors/headers, and Authentication Guard (`auth.guard.ts`)
* **Key Components:**
  * `landing` - Welcome screen and introductory layout
  * `auth` - User login and registration forms
  * `dashboard` - Displays movies and available showtimes, and user balance details
  * `booking` - Grid-based interactive seat selection and booking confirmation
  * `admin` - Panel to add/delete movies and configure showtimes (restricted to `Admin` role users)

---

## 📋 Prerequisites

Before running the application, make sure you have the following installed:

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
2. **[Node.js (v18 or higher)](https://nodejs.org/)** & **npm**
3. **SQL Server LocalDB**

---

## 🚀 Setup & Execution Guide

### Step 1: Set Up and Run the Backend (`Backend`)

1. Open your terminal and navigate to the backend folder:
   ```bash
   cd Backend
   ```

2. Restore the required NuGet packages:
   ```bash
   dotnet restore
   ```

3. **Run the API:**
   Start the backend server:
   ```bash
   dotnet run
   ```
   * By default, the API will be available at: **`http://localhost:5231`** (or HTTPS at `https://localhost:7072`)
   * On startup, the database `CineBookingDb` will automatically create, and initial showtimes will seed along with an admin account:
     * **Username:** `admin`
     * **Password:** `admin123`

---

### Step 2: Set Up and Run the Frontend (`cine-booking`)

1. Open a new terminal window and navigate to the frontend folder:
   ```bash
   cd cine-booking
   ```

2. Install the npm packages and dependencies:
   ```bash
   npm install
   ```

3. **Run the Angular App:**
   Start the development server:
   ```bash
   npm run start
   ```
   *(or `ng serve`)*

4. Open your browser and navigate to **`http://localhost:4200`** to interact with the cinema booking system.

---

## 📁 Directory Structure

```text
Movie Booking Management/
├── README.md               # Project documentation
├── Backend/                # C# .NET 10.0 Backend
│   ├── Controllers/        # Shows & Bookings controllers
│   ├── Data/               # DB Context configuration
│   ├── DTOs/               # Data Transfer Objects
│   ├── Models/             # Entity models (Movie, Showtime, Booking, User, etc.)
│   ├── Middleware/         # GlobalExceptionMiddleware
│   ├── Program.cs          # API entry point & JWT authentication configuration
│   ├── appsettings.json    # Connection strings and JWT options
│   └── CineBooking.Api.csproj
└── cine-booking/           # Angular 18 Frontend
    ├── src/
    │   ├── app/
    │   │   ├── components/ # landing, auth, dashboard, booking, admin
    │   │   ├── models/     # data interfaces
    │   │   ├── services/   # auth.service.ts, movie.service.ts
    │   │   ├── auth.guard.ts # Auth Route Guard
    │   │   └── app.routes.ts # Angular client routing rules
    └── package.json        # Frontend configuration and npm scripts
```
