# 📚 BookShelf Management System

A modern, full-stack web application for managing a collection of books. The project features an **ASP.NET Core Web API (C#)** backend built with .NET 10 and an **Angular 21** frontend. 

---

## 🏗️ Project Architecture & Tech Stack

This repository is split into two main sections:

### 1. Backend (`/BookAPI`)
* **Framework:** ASP.NET Core Web API (.NET 10)
* **Database ORM:** Entity Framework Core 10.0.8 (SQL Server LocalDB)
* **Documentation & Testing:** Swagger / OpenAPI (via Swashbuckle)
* **Key Features:** 
  * Full CRUD (Create, Read, Update, Delete) endpoints.
  * Server-side models validation (e.g., character length checks, price ranges, and a custom `[PastDate]` validator to prevent future publication dates).
  * Database-level check constraints for data integrity.
  * Cross-Origin Resource Sharing (CORS) configured for local development.

### 2. Frontend (`/book-ui`)
* **Framework:** Angular 21
* **Styles:** SCSS / Custom CSS & FontAwesome Icons
* **State & HTTP:** RxJS & Angular HttpClient
* **Testing:** Vitest & Angular Testing Utilities
* **Key Features:**
  * Clean, interactive dashboard displaying all books.
  * Dynamic form for adding and editing books with real-time validation feedback.
  * Delete confirmations and seamless updates.

---

## 📋 Prerequisites

Before running the application, make sure you have the following installed:

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
2. **[Node.js (v18 or higher)](https://nodejs.org/)** & **npm**
3. **SQL Server LocalDB** (usually installed automatically with Visual Studio's ".NET desktop development" or "ASP.NET and web development" workloads. Alternatively, you can run MS SQL Express LocalDB).
4. **EF Core CLI tool** (optional, for applying migrations):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

---

## 🚀 Setup & Execution Guide

### Step 1: Set Up and Run the Backend (`BookAPI`)

1. Open your terminal and navigate to the backend folder:
   ```bash
   cd BookAPI
   ```

2. Restore the required NuGet packages:
   ```bash
   dotnet restore
   ```

3. **Apply Database Migrations:**
   Run the following command to create the database (`BookDB`) and apply the tables and check constraints:
   ```bash
   dotnet ef database update
   ```

4. **Run the API:**
   Start the backend server:
   ```bash
   dotnet run
   ```
   * By default, the API will be available at: `http://localhost:5193`
   * You can access the **Swagger interactive documentation** at: **`http://localhost:5193/swagger`**

---

### Step 2: Set Up and Run the Frontend (`book-ui`)

1. Open a new terminal window and navigate to the frontend folder:
   ```bash
   cd book-ui
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

4. Open your browser and navigate to **`http://localhost:4200`** to view the application.

---

## 🧪 Running Tests

* **Backend Tests:** Run dotnet test suite (if applicable):
  ```bash
  dotnet test
  ```
* **Frontend Tests:** Run Vitest unit tests:
  ```bash
  cd book-ui
  npm run test
  ```

---

## 📁 Repository Structure

```text
BookShelf Management System/
├── README.md               # Main project documentation
├── .gitignore              # Standard git ignore rules (Node & .NET)
├── BookAPI/                # C# .NET 10.0 Backend
│   ├── Controllers/        # API Controllers (BooksController)
│   ├── Data/               # DB Context configuration (AppDbContext)
│   ├── Migrations/         # EF Core migrations
│   ├── Models/             # Entity models (Book)
│   ├── Repositories/       # Database access layers
│   ├── Program.cs          # API entry point & services setup
│   └── BookAPI.csproj      # Backend project configuration
└── book-ui/                # Angular 21 Frontend
    ├── src/
    │   ├── app/
    │   │   ├── core/       # Services & interfaces
    │   │   ├── pages/      # Book List & Book Form components
    │   │   └── app.routes.ts # Frontend routing rules
    └── package.json        # Frontend configuration and npm scripts
```
