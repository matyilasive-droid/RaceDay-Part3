# RaceDay — Part 3 MVC + Part 2 API

This repository now includes the supplied Part 2 ASP.NET Core REST API source, the Part 3 ASP.NET Core MVC frontend, and both test projects. The MVC frontend is designed to use HTTP API calls; it does not contain an Entity Framework context or a direct database connection.

## Solution structure

```text
RaceDay/
├── .github/workflows/ci.yml
├── Dockerfile                         # MVC web container
├── RaceDay.sln
├── src/
│   ├── RaceDay.Api/                   # supplied Part 2 API + API Dockerfile
│   └── RaceDay.Web/                   # Part 3 MVC frontend
├── tests/
│   ├── RaceDay.Api.Tests/
│   └── RaceDay.Web.Tests/
└── docs/                              # rubric and API verification notes
```

## Stage 1 — local setup

Follow [`docs/LOCAL-DEVELOPMENT-SETUP.md`](docs/LOCAL-DEVELOPMENT-SETUP.md) first. It covers VS Code, both required .NET SDKs, SSMS database verification, the API connection string, and the correct local startup ports. The API HTTP profile runs at `http://localhost:5291`; the MVC profile runs at `http://localhost:5080`.

## Prerequisites

- .NET 8 SDK for the MVC application and .NET 10 SDK for the supplied API/API tests.
- SQL Server with the RaceDay schema expected by `RaceDayProg6212Context`.
- A configured SQL Server connection string for the API.

Set the API connection string before starting it. For PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=RaceDayProg6212;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet run --project src/RaceDay.Api --launch-profile http
```

Then start the MVC application in a second terminal:

```powershell
dotnet run --project src/RaceDay.Web --launch-profile RaceDay.Web
```

The MVC configuration points to the API HTTP launch profile at `http://localhost:5291/`. The API's database must already exist; this archive does not include a database backup or migration script.

## Build and test

```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln --configuration Release
dotnet test RaceDay.sln --configuration Release
```

The GitHub Actions workflow installs both SDK versions, restores and builds the complete solution, runs both test projects, and builds both Docker images. A green workflow has not been verified from this local archive; push the repository and inspect the Actions run.

## Docker

Build the MVC and API images separately:

```bash
docker build -t raceday-web .
docker build -f src/RaceDay.Api/Dockerfile -t raceday-api .
```

Run the API with a reachable SQL Server connection string:

```bash
docker run --rm -p 8081:8081 -e ConnectionStrings__DefaultConnection="<SQL_SERVER_CONNECTION_STRING>" raceday-api
```

Run the MVC frontend and point it to the API URL reachable from inside the container:

```bash
docker run --rm -p 8080:8080 -e Api__BaseUrl="http://host.docker.internal:8081/" raceday-web
```

On Linux, add `--add-host=host.docker.internal:host-gateway` to the MVC `docker run` command when the API is running on the host.

## Important integration verification

The Part 2 API source is now included in the solution and CI build. However, inclusion is not the same as end-to-end compatibility. The supplied API currently uses cookie authentication and controller routes based on `[controller]`, while the MVC client expects a bearer token and several `api/...` route shapes (including `/api/users/me`, event-scoped category/enrolment routes, and participant-specific results). The supplied API also has no Azure Blob upload endpoints or persisted image URL fields. These mismatches must be resolved and tested against the actual database/API contract before claiming the full Part 3 functionality is complete. See `docs/SUBMISSION-AUDIT.md` and `docs/API-VERIFICATION-CHECKLIST.md`.

## Submission evidence still required

- Push all repository commits to the required GitHub remote and verify the history there (the included local Git history contains 20 commits before these integration changes).
- Capture a screenshot of a successful GitHub Actions run after the push.
- Record and add the unlisted YouTube demonstration URL showing MVC, API calls, authentication/roles, database operations, Azure Blob integration, Docker, and tests.
- Verify the actual database connection and execute the end-to-end checks in the API verification checklist.

Do not submit placeholder CI/video evidence as if it were completed.
