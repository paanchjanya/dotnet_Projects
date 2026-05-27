# 📖 Library Management System

A full-stack library management application designed for tracking and managing book availability. The application features an **ASP.NET Core Web API (.NET 10)** backend and an **Angular 21** frontend.

---

## 🏗️ Project Architecture & Tech Stack

This project is divided into two primary directories:

### 1. Backend (`/backend`)
* **Framework:** ASP.NET Core Web API (.NET 10)
* **Database ORM:** Entity Framework Core 10.0.8 (SQL Server LocalDB)
* **API Documentation:** Swagger / OpenAPI UI
* **Key Features:**
  * Clean Architecture utilizing the Repository Pattern (`IBookRepository` & `SqlBookRepository`).
  * Automatic Database Creation (`IronhideLibrary`) and Seeding of default books on application startup.
  * Cross-Origin Resource Sharing (CORS) configured for localhost frontend requests.
  * Endpoints covering full CRUD operations:
    * `GET /api/books` - Retrieve all books
    * `GET /api/books/{id}` - Retrieve a specific book by ID
    * `POST /api/books` - Add a new book
    * `PUT /api/books/{id}` - Update a book
    * `DELETE /api/books/{id}` - Delete a book

### 2. Frontend (`/frontend`)
* **Framework:** Angular 21
* **Styles:** Custom CSS layout, Header, and responsive forms
* **State & HTTP:** RxJS & Angular HttpClient
* **Key Components:**
  * `header` - Global navigation
  * `book-list` - Lists books with availability status, pricing, and a delete action
  * `book-form` - Form for adding and updating books in the catalog

---

## 📋 Prerequisites

Before running the application, make sure you have the following installed:

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)**
2. **[Node.js (v18 or higher)](https://nodejs.org/)** & **npm**
3. **SQL Server LocalDB** (usually installed with Visual Studio's ".NET desktop development" workload)

---

## 🚀 Setup & Execution Guide

### Step 1: Set Up and Run the Backend (`backend`)

1. Open your terminal and navigate to the backend folder:
   ```bash
   cd backend
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
   * By default, the API runs at: **`http://localhost:5043`** (or HTTPS at `https://localhost:7272`)
   * On startup, the server automatically checks if the database `IronhideLibrary` exists, creates it, and seeds default books if the table is empty.
   * Access the **Swagger interactive documentation** at: **`http://localhost:5043/swagger`**

---

### Step 2: Set Up and Run the Frontend (`frontend`)

1. Open a new terminal window and navigate to the frontend folder:
   ```bash
   cd frontend
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

4. Open your browser and navigate to **`http://localhost:4200`** to interact with the Library System.

---

## 📁 Directory Structure

```text
Library Management System/
├── README.md               # Project documentation
├── backend/                # C# .NET 10.0 Backend
│   ├── Controllers/        # BooksController
│   ├── Models/             # Book entity model
│   ├── Services/           # IBookRepository & SqlBookRepository
│   ├── Program.cs          # API entry point & services configuration
│   ├── appsettings.json    # Database connection string configuration
│   └── backend.csproj      # Backend project configuration
└── frontend/               # Angular 21 Frontend
    ├── src/
    │   ├── app/
    │   │   ├── components/ # header, book-list, book-form
    │   │   ├── models/     # book interface definition
    │   │   ├── services/   # book.service.ts
    │   │   └── app.routes.ts # Frontend routing rules
    └── package.json        # Frontend configuration and npm scripts
```
