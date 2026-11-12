# Exam Planner

A web application for planning exams at VIA University College. Exam planners import the planning data
exported from the university's system (an Excel file), allowing for reviewing and correcting it, and (as the project grows)
build exam schedules from it.

> **Status: work in progress.** Login, the Excel import and the course overview work end to end.
> Scheduling, conflict checking and the other use cases in [the API contract](Docs/API_CONTRACT.md)
> are planned and not built yet. See [Status and known limitations](#status-and-known-limitations).

## What works today

- **Login** with email and password. The API issues a JWT; the website keeps it in a cookie.
- **Roles.** Exam planners (called `Admin` in the API) can import and edit. Teachers and students can log in
  but have no features of their own yet.
- **Excel import.** A planner uploads the export from the university's system. Every row becomes a planning
  element, a course and a class. Bad rows are skipped and reported with their row number; the rest is
  imported. Importing the same file again is safe. See [Importing data](#importing-data).
- **Imported data page.** Lists the imported courses and lets a planner correct a course's name and code.
- **Calendar pages** (create and edit a plan). These are user-interface work for the scheduling that is
  still to be built on the backend.

## Tech stack

| Part | Technology |
|---|---|
| Website | React 19, TypeScript, Vite 8, React Router, axios |
| API | C# on .NET 10, ASP.NET Core controllers, JWT bearer authentication |
| Database | PostgreSQL, Entity Framework Core 10 (migrations) |
| Excel reading | ClosedXML |
| Tests | xUnit, in-memory SQLite and real PostgreSQL, ASP.NET Core test host |
| Running everything | Docker Compose |

## Repository layout

```text
.
├── docker-compose.yml          Starts database, migrations, API and website together
├── .env.example                Template for the .env file that Docker Compose reads
├── Docs/
│   ├── API_CONTRACT.md         The agreed interface between website and API
│   └── openapi.yaml            The same contract as OpenAPI
├── Server/
│   ├── ExamPlannerServer/      The API: controllers, authentication, Excel import (Import/)
│   ├── DatabaseConnection/     Entities, DbContext, repositories and EF Core migrations
│   └── TesProject/             All backend tests
└── Website/
    └── exam-planner-app/       The React website
```

## Getting started

### Option 1: Docker (recommended)

Need [Docker Desktop](https://www.docker.com/products/docker-desktop/) (on Windows with the WSL 2 backend).

1. **Create a `.env` file** in the repository root (the same folder as `docker-compose.yml`). It is ignored
   by git. Copy `.env.example` and fill it in, for example:

   ```text
   APP_VERSION=1.0.0
   APP_ENV=development
   APP_PORT=8080

   # The PostgreSQL that Docker creates. It is separate from any PostgreSQL installed on your computer.
   DB_HOST=postgres
   POSTGRES_PORT=5433
   DB_PORT=5432
   DB_NAME=ExamPlanner
   DB_USER=postgres
   DB_PASSWORD=choose-a-password
   ```

   `POSTGRES_PORT` is the port on *your* computer. Use something other than 5432 if you also have PostgreSQL
   installed locally. Do not set `VITE_API_URL` for Docker: the website reaches the API through its built-in proxy.

2. **Start everything** from the repository root:

   ```bash
   docker compose up --build
   ```

   The first build takes several minutes. Services start in this order: PostgreSQL, database migrations,
   API, website.

3. **Open the website** at <http://localhost:4040> and log in as the development planner account
   (see [Configuration](#configuration)).

| Service | Address |
|---|---|
| Website | <http://localhost:4040> |
| API | <http://localhost:8080> (OpenAPI document at `/openapi/v1.json` in development) |
| PostgreSQL | `localhost:5433` (the `POSTGRES_PORT` above) |

Everyday commands:

| What | Command |
|---|---|
| Start again | `docker compose up` |
| Rebuild after code changes | `docker compose up --build` |
| Stop, keep the data | `docker compose down` |
| Stop and **delete the database** | `docker compose down -v` |
| Follow the API log | `docker compose logs -f api` |

### Option 2: Run the parts 

Need the .NET 10 SDK, Node.js, and a PostgreSQL server you can create a database in.

**1. Create or update the database.** The migrations read the connection from environment variables
(or from a `.env` file in the repository root):

```powershell
dotnet tool install --global dotnet-ef --version 10.0.11
```

```powershell
$env:DB_HOST="localhost"; $env:DB_PORT="5432"; $env:DB_NAME="ExamPlanner"; $env:DB_USER="postgres"; $env:DB_PASSWORD="your-password"
dotnet ef database update --project Server/DatabaseConnection --startup-project Server/ExamPlannerServer
```

**2. Start the API on port 8080** (the website's dev proxy expects it there). `--no-launch-profile` matters:
without it `dotnet run` ignores `ASPNETCORE_URLS` and uses port 5145.

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"; $env:ASPNETCORE_URLS="http://localhost:8080"
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=ExamPlanner;Username=postgres;Password=your-password"
dotnet run --project Server/ExamPlannerServer --no-launch-profile
```

**3. Start the website:**

```bash
cd Website/exam-planner-app
npm install
npm run dev
```

Open the address Vite prints (normally <http://localhost:5173>).

## Configuration

**Docker / database** (`.env`, see `.env.example`):

| Variable | Meaning |
|---|---|
| `DB_NAME`, `DB_USER`, `DB_PASSWORD` | Database name and login. Docker creates the database with these. |
| `DB_PORT` | PostgreSQL's port inside Docker (normally `5432`). |
| `POSTGRES_PORT` | The port on your computer that reaches the Docker database. |
| `APP_VERSION` | Tag for the Docker images. |

**API** (`Server/ExamPlannerServer/appsettings*.json`, or environment variables with `__` instead of `:`):

| Setting | Meaning |
|---|---|
| `ConnectionStrings:DefaultConnection` | The database connection. Docker Compose sets it for you. |
| `Jwt:Key` | Signing key for login tokens, at least 32 characters. **Never commit a real key**; set `Jwt__Key` in production. |
| `Auth:AdminEmails` | The accounts that get the `Admin` (exam planner) role. |
| `Seed:AdminEmail`, `Seed:AdminPassword`, `Seed:AdminName` | An account created at startup if it does not exist. |

`appsettings.Development.json` contains a development-only planner account and signing key so that a fresh
checkout can log in straight away. Look in that file for the account (`test.planner@via.dk`). It applies only
when `ASPNETCORE_ENVIRONMENT` is `Development`, which is what Docker Compose sets. In any real deployment,
supply your own `Jwt__Key` and seed values as environment variables.

## Importing data

Log in as a planner, open the import page and upload the Excel export from the university's system
(the "Planlægningsuddannelseselement" view). Only `.xlsx` and `.xlsm` files up to 25 MB are accepted.

**How the file is read**

- The data table is found by its column names, not by position or sheet name, so a re-export with reordered
  or extra columns still works. The required columns are listed in
  `Server/ExamPlannerServer/Import/ExcelImportParser.cs`.
- The Excel file itself is never stored; only its data is.

**What each row becomes**

| Result | Where it comes from |
|---|---|
| Planning element | A copy of the row, identified by the id in column A. If the row's checksum changed since the last import, the element is updated. |
| Class | One per row. Class code and nickname = `Kaldenavn`; dates from the file. |
| Course | Programme prefix = the first two letters of `Kaldenavn`. Code = the first word of `Navn`, up to a hyphen. Name = `Navn` without that code. Campus = the end of `Aktivitetsudbud` (`..., Campus Horsens`). Two rows are the same course when prefix and code match. |

The derivation rules live in one file: `Server/ExamPlannerServer/Import/CourseDerivation.cs`.

**Rules to know**

- **Bad rows are skipped, not fatal (do not raise errors).** A row with a missing value, a wrong type, decimal ECTS or an unknown
  assessment type is reported (with its row number) and skipped; all other rows are imported.
- **Re-importing is safe.** Existing courses are never overwritten, so corrections made on the Imported data
  page survive. A row finds its course through its class, so a renamed course is not duplicated.
- **All or nothing for the database.** Everything one import writes happens in a single transaction.
- **Hand-filled values stay.** Semester and priority are left at `0` for planners to fill in.

## API

All endpoints are under `/api/v1`. Errors have the shape `{ "code": "...", "message": "..." }`; the website
decides what to do from `code`. The full contract is in [`Docs/API_CONTRACT.md`](Docs/API_CONTRACT.md) and
[`Docs/openapi.yaml`](Docs/openapi.yaml). Not every endpoint in the contract exists yet.

| Endpoint | Who | What |
|---|---|---|
| `POST /auth/login` | anyone | Log in; returns `accessToken` and the user |
| `GET /auth/me` | logged in | The current user |
| `POST /imports` | Admin | Upload and import the Excel file (`multipart/form-data`, field `file`) |
| `GET /courses`, `GET /courses/{id}` | logged in | The imported courses, with their class count |
| `PATCH /courses/{id}` | Admin | Correct a course's name and code |

## Database

PostgreSQL, accessed with Entity Framework Core. Entities, the `AppDbContext`, repositories and migrations are
in `Server/DatabaseConnection`. To add a migration after changing an entity:

```powershell
dotnet ef migrations add NameOfTheChange --project Server/DatabaseConnection --startup-project Server/ExamPlannerServer
```

Docker Compose applies pending migrations automatically at startup (the `migrations` service). Commit the generated
files in `Server/DatabaseConnection/Migrations`.

## Testing

```bash
dotnet test Server/TesProject
```

The backend tests cover the Excel reader, the import service against a database, the controllers, and the
whole API over HTTP (login, roles, import, courses). By default the database tests run on in-memory SQLite.
To run them on real PostgreSQL too, give the tests a connection to a server where they may create and drop
temporary databases (each test gets its own and removes it afterwards; local normal database is never used):

```powershell
$env:EXAMPLANNER_TEST_PG = "Host=localhost;Port=5432;Username=postgres;Password=your-password;Database=postgres"
```

A few tests read a real export from disk. They skip themselves if the file is missing. The path is the constant
`path` in `Server/TesProject/ExcelTest.cs`; set it to where your copy of the export is (do not commit exports that
contain real data).

The CI workflow builds the API and the website on every pull request. It runs `dotnet test` inside
`Server/ExamPlannerServer`, which has no tests of its own, so `Server/TesProject` is currently run locally.

For the website: `npm run lint` and `npm run build` (in `Website/exam-planner-app`).

## Working on the project

- Branch from `develop`, name branches `feature/...` or `fix/...`, and open pull requests into `develop`.
- Keep `.env` out of git (it is ignored). Never commit passwords or signing keys.
- Update `Docs/API_CONTRACT.md` and `Docs/openapi.yaml` when the website/API interface changes.
- If you use Rider or Visual Studio, open the project through a solution file. The repository does not contain one;
  create your own and keep it out of git (for example by adding `ExamPlanner.sln` to your local `.git/info/exclude`):

  ```bash
  dotnet new sln -n ExamPlanner --format sln
  dotnet sln ExamPlanner.sln add Server/DatabaseConnection Server/ExamPlannerServer Server/TesProject
  ```

## Troubleshooting

| Problem | Likely cause and fix |
|---|---|
| "Cannot connect to the Docker daemon" | Docker Desktop is not running. Start it and wait for "Engine running". |
| "port is already allocated" | Something else uses 8080, 4040 or your `POSTGRES_PORT`. Stop it or change the port. |
| Docker image build fails strangely | Delete leftover `bin` and `obj` folders under `Server` and the `node_modules` folder under `Website/exam-planner-app` (Windows build leftovers get copied into the Linux image), then rebuild. |
| The website opens but nothing loads | The API is not running or not reachable. Check `docker compose logs -f api`. When running without Docker, the API must be on port 8080. |
| `npm run dev` says "Cannot find native binding" | An npm optional-dependency problem seen on Windows. Delete `node_modules` and run `npm install` again, or use Docker. |
| Login fails after `docker compose down -v` | The database was wiped. Start again; the migrations and the development account are recreated at startup. |

## Status and known limitations

- **Planned, not built:** schedule generation, conflict detection, rescheduling, student and examiner
  assignment, re-exam eligibility, schedule approval and export (all described in the API contract).
- **Import:** it creates courses, classes and planning elements. It does not yet create the exam records
  (assessment events), and the export contains no teacher data. A course that runs on two campuses keeps the
  campus of the first row that mentions it.
- **Roles:** teachers and students can log in, but only planners have features. After login a planner lands on
  the main page; other roles land on a placeholder page until their pages exist.
