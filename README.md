# 🛠️ Multi-Project Workspace (.NET Core & Angular)

Welcome to this monorepo/workspace featuring three modern, full-stack web applications. Each application utilizes an **ASP.NET Core Web API (.NET 10)** backend and an **Angular** frontend, demonstrating database seeding, entity validations, CRUD operations, and security protocols.

---

## 📂 Repository Contents

This repository hosts the following three independent projects:

### 1. [📚 BookShelf Management System](./) (Root Workspace Folder)
* **Backend:** ASP.NET Core Web API (.NET 10) in `BookAPI/`
* **Frontend:** Angular 21 in `book-ui/`
* **Features:** Custom field validators (e.g., publish date validation), EF Core SQLite/SQL Server model constraints, and interactive book logging UI.
* **Documentation:** Detailed instructions are provided in the section below.

### 2. [📖 Library Management System](../Library%20Management%20System)
* **Backend:** ASP.NET Core Web API (.NET 10) in `Library Management System/backend/`
* **Frontend:** Angular 21 in `Library Management System/frontend/`
* **Features:** Database auto-seeding, Repository pattern implementation, and clean status tracking UI.
* **Detailed Readme:** [Library Management System README](../Library%20Management%20System/README.md)

### 3. [🎬 Movie Booking Management](../Movie%20Booking%20Management)
* **Backend:** ASP.NET Core Web API (.NET 10) in `Movie Booking Management/Backend/`
* **Frontend:** Angular 18 in `Movie Booking Management/cine-booking/`
* **Features:** JWT authentication, role-based authorization (`Admin`/`Customer`), and interactive grid seat-selection UI.
* **Detailed Readme:** [Movie Booking Management README](../Movie%20Booking%20Management/README.md)

---

## 📋 General Workspace Prerequisites

Before launching any of the applications, ensure you have the following installed on your system:

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
2. **[Node.js (v18 or higher)](https://nodejs.org/)** & **npm**
3. **SQL Server LocalDB** (standard with Visual Studio's ".NET desktop development" workload)
4. **Entity Framework Core CLI Tool** (required for creating/applying database migrations):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

---

## 🚀 "How To" Setup & Execution Guides

Below are the instructions to run each system. You must run the backend and frontend of a project in separate terminal windows.

### Project A: BookShelf Management System

#### 1. Start the Backend (`BookAPI`)
```bash
cd BookAPI
dotnet restore
dotnet ef database update
dotnet run
```
* **API Port:** `http://localhost:5193`
* **Swagger Documentation:** `http://localhost:5193/swagger`

#### 2. Start the Frontend (`book-ui`)
```bash
cd book-ui
npm install
npm run start
```
* **Client App URL:** `http://localhost:4200`

---

### Project B: Library Management System

#### 1. Start the Backend (`Library Management System/backend`)
```bash
cd "Library Management System/backend"
dotnet restore
dotnet run
```
* **API Port:** `http://localhost:5043`
* **Swagger Documentation:** `http://localhost:5043/swagger`
* *Note: The database `IronhideLibrary` auto-creates and seeds itself on run.*

#### 2. Start the Frontend (`Library Management System/frontend`)
```bash
cd "Library Management System/frontend"
npm install
npm run start
```
* **Client App URL:** `http://localhost:4200`

---

### Project C: Movie Booking Management

#### 1. Start the Backend (`Movie Booking Management/Backend`)
```bash
cd "Movie Booking Management/Backend"
dotnet restore
dotnet run
```
* **API Port:** `http://localhost:5231` (HTTPS at `https://localhost:7072`)
* *Note: The database `CineBookingDb` auto-creates and seeds movies, showtimes, and an admin user: **Username:** `admin`, **Password:** `admin123`.*

#### 2. Start the Frontend (`Movie Booking Management/cine-booking`)
```bash
cd "Movie Booking Management/cine-booking"
npm install
npm run start
```
* **Client App URL:** `http://localhost:4200`

---

## 🧪 Testing the Apps
* **Backend C# Projects:** Run `dotnet test` from any of the API root folders.
* **Frontend Angular Projects:** Run `npm run test` or `ng test` from any of the frontend folders.
