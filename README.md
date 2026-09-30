# 🏥 CareNet: Healthcare Appointment & Doctor Management

A full-stack appointment booking platform built with **ASP.NET Core 8** and **Angular 18**. Patients browse doctors, see free time slots, and book appointments; admins manage the doctor directory.

![.NET](https://img.shields.io/badge/.NET-8-512BD4) ![Angular](https://img.shields.io/badge/Angular-18-DD0031) ![SQL Server](https://img.shields.io/badge/SQL%20Server-EF%20Core-CC2927) ![JWT](https://img.shields.io/badge/Auth-JWT-000000)

## ✨ Features
- **JWT authentication** with role-based authorization (`Patient`, `Admin`)
- **Doctor directory**: public browsing, admin-only create / update / delete
- **Slot-based booking**: live availability per doctor and date (hourly, 09:00 to 16:00)
- **Double-booking protection**: a unique database index on `(DoctorId, StartsAt)`, so two patients can't take the same slot even under concurrent requests
- **Swagger UI** with Bearer-token support
- Passwords hashed with ASP.NET Core `PasswordHasher`

## 🧱 Tech Stack
| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API, Entity Framework Core, SQL Server |
| Auth | JWT Bearer, role-based policies |
| Frontend | Angular 18 (standalone components, signals, HTTP interceptor) |
| Docs | Swagger / OpenAPI |

## 🔌 API Endpoints
| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register a patient |
| POST | `/api/auth/login` | Public | Login, returns JWT |
| GET | `/api/doctors` | Public | List doctors |
| GET | `/api/doctors/{id}` | Public | Get one doctor |
| GET | `/api/doctors/{id}/slots?date=YYYY-MM-DD` | Public | Free slots for a day |
| POST / PUT / DELETE | `/api/doctors[/{id}]` | Admin | Manage doctors |
| POST | `/api/appointments` | Patient | Book a slot |
| GET | `/api/appointments/mine` | Patient | My appointments |
| DELETE | `/api/appointments/{id}` | Owner | Cancel appointment |

## 🚀 Getting Started

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download), [Node.js 18+](https://nodejs.org), SQL Server LocalDB (installed with Visual Studio) or any SQL Server instance.

### Backend
```bash
cd backend/CareNet.Api
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet run
```
The database is created automatically on startup. Swagger: http://localhost:5080/swagger

> Using another SQL Server? Edit `ConnectionStrings:Default` in `appsettings.json`.
> **Production:** set the JWT key through an environment variable (`Jwt__Key`), never commit a real secret.

### Frontend
```bash
cd frontend
npm install
npx ng serve
```
Open http://localhost:4200

### Demo accounts
- Admin: `admin@carenet.com` / `Admin@123` (seeded for development only)
- Patient: register from the UI

## 🗂 Project Structure
```
CareNet/
├── backend/CareNet.Api/   # Controllers, EF Core models, JWT setup
└── frontend/              # Angular app
```

## 🗺 Roadmap
- [ ] Unit tests (xUnit) for booking rules
- [ ] Service/Repository layering
- [ ] Doctor role: manage own schedule
- [ ] SignalR real-time slot updates
- [ ] Docker Compose + CI with GitHub Actions
- [ ] Live deployment

## 👩‍💻 Author
**Shahd Ashraf** · [LinkedIn](https://linkedin.com/in/shahd-ashraf-5b13592a3) · [GitHub](https://github.com/shahdood1212)
