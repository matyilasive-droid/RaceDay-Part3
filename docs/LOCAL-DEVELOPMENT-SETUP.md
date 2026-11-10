# RaceDay Part 3 — Local Development Setup (Stage 1)

This guide prepares the supplied Part 2 API and Part 3 MVC frontend for local development in VS Code with SQL Server Management Studio (SSMS) 2022.

> Important: this stage verifies the local toolchain, database connection and API startup. It does **not** yet certify MVC login/API compatibility. The supplied API currently uses cookie authentication while the MVC client expects a token; that is a Stage 2 correction.

## 1. Keep a clean working copy

1. Extract the project ZIP to a normal folder, for example `C:\Projects\RaceDay`.
2. Open the folder containing `RaceDay.sln` in VS Code (`File > Open Folder`).
3. Keep the original ZIP as an untouched backup.
4. Install the VS Code **C# Dev Kit** extension (it also installs/supports the C# language tooling).

## 2. Install the required .NET SDKs

The supplied projects target different frameworks:

- `RaceDay.Api` and `RaceDay.Api.Tests`: .NET 10 (`net10.0`).
- `RaceDay.Web` and `RaceDay.Web.Tests`: .NET 8 (`net8.0`).

Install both the .NET 8 SDK and .NET 10 SDK. Open a new PowerShell terminal and verify:

```powershell
dotnet --list-sdks
dotnet --list-runtimes
```

You should see an `8.0.x` SDK and a `10.0.x` SDK. If one is missing, install it from the official .NET download page, then restart VS Code.

## 3. Check SQL Server in SSMS 2022

1. Open SQL Server Management Studio 2022.
2. Connect to the SQL Server instance that contains your RaceDay Part 2 database. Common server names include `localhost`, `localhost\SQLEXPRESS`, or `.(localdb)\MSSQLLocalDB`; use the exact instance configured on your computer.
3. In Object Explorer, expand **Databases** and look for `RaceDayProg6212` (the database name used in the examples below).
4. Do not create a blank database if your Part 2 database already exists. The API needs the actual tables and view, not merely a database with the correct name.
5. Select the RaceDay database, click **New Query**, and run this verification query:

```sql
SELECT DB_NAME() AS CurrentDatabase;

SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.VIEWS
ORDER BY TABLE_NAME;
```

The API's EF Core context expects these database objects:

- Tables: `Categories`, `Enrolments`, `Event`, `EventTypes`, `Result`, `Users`.
- View: `vw_EventCategories`.

If those objects are missing, stop here and restore/use the correct Part 2 database or its supplied SQL script/backup. Do not try to run the API against an empty database.

## 4. Configure the API database connection (PowerShell)

In VS Code, open **Terminal > New Terminal** and make sure the terminal is PowerShell. Set the connection string for the current terminal session. Replace the server name with the exact name shown in SSMS.

For a default/local SQL Server instance using Windows authentication:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=RaceDayProg6212;Trusted_Connection=True;TrustServerCertificate=True;"
```

For SQL Server Express, for example:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=RaceDayProg6212;Trusted_Connection=True;TrustServerCertificate=True;"
```

Do not commit passwords or private connection strings to Git. The environment variable above is temporary and applies only to that terminal window. If you close the terminal, set it again before starting the API.

## 5. Restore and build the solution

Run these commands from the folder containing `RaceDay.sln`:

```powershell
dotnet --info
dotnet restore .\RaceDay.sln
dotnet build .\RaceDay.sln --configuration Debug
```

If restore/build fails, copy the **first complete error** from the terminal. Do not continue to database or MVC testing until the build issue is resolved.

## 6. Start the Part 2 API first

Keep the same PowerShell terminal (with the database connection variable set) and run:

```powershell
dotnet run --project .\src\RaceDay.Api\RaceDay.Api.csproj --launch-profile http
```

The supplied API HTTP launch profile is `http://localhost:5291`. Leave this terminal running. In a browser, open:

- `http://localhost:5291/swagger` — Swagger UI (Development environment only).
- `http://localhost:5291/WeatherForecast` — simple API reachability check.

A successful WeatherForecast response confirms that the API process is reachable; it does **not** yet prove database connectivity or that all protected API routes work.

If the API reports a SQL exception, confirm the connection string server/instance and database name in SSMS, and confirm the required tables/view exist.

## 7. Start the MVC frontend in a second terminal

Open a **second** PowerShell terminal in the same solution folder. The MVC app's local configuration now points to the API's actual HTTP launch port (`http://localhost:5291/`). Run:

```powershell
dotnet run --project .\src\RaceDay.Web\RaceDay.Web.csproj
```

The supplied MVC launch profile uses `http://localhost:5080`. Open `http://localhost:5080` in your browser.

At this stage, the homepage should load and the API should be independently reachable. Do not treat successful homepage rendering as proof that login, role navigation, event CRUD, enrolments or results work; those depend on the API contract and are addressed in Stage 2 onward.

## 8. Run tests

From a third terminal (or after stopping the applications with `Ctrl+C`):

```powershell
dotnet test .\RaceDay.sln --configuration Debug
```

Record the pass/fail summary. If a test fails, retain the full error output for diagnosis.

## 9. What to send back before Stage 2

Please provide:

1. Output of `dotnet --list-sdks`.
2. Your SQL Server **Server name** from the SSMS Connect dialog (do not send a password).
3. Whether `RaceDayProg6212` exists and whether the six tables plus `vw_EventCategories` are present.
4. Whether `/swagger` opens and whether `/WeatherForecast` returns JSON.
5. The first build/test error, if any (paste text or upload a screenshot).

Do not share database passwords, API keys, or other secrets.
