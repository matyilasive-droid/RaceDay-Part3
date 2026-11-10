# RaceDay — Part 3 (ASP.NET Core MVC front end)

An ASP.NET Core 8 MVC front end for the RaceDay event platform. It calls the separately deployed REST API over HTTP.
The uploaded Part 3 ZIP does **not** contain the API implementation or database project; the API plan is used here as
the client contract. There is no local DbContext or direct database connection in this MVC application.

| | |
|---|---|
| CI status | ![CI](https://github.com/<YOUR-USER>/<YOUR-REPO>/actions/workflows/ci.yml/badge.svg) |
| CI screenshot | `docs/ci-green-build.png` *(add your screenshot here)* |
| Demo video (unlisted) | `<PASTE YOUTUBE LINK>` |

## Folder structure

```
RaceDay/
├── .github/workflows/ci.yml        # builds API + MVC, runs tests, builds Docker image
├── Dockerfile                      # multi-stage build for the MVC app
├── RaceDay.sln
├── src/
│   ├── RaceDay.Api/                # ← your Part 2 API project goes here
│   └── RaceDay.Web/                # MVC app
│       ├── Controllers/            # Account, Home, Events, MyEnrolments, MyResults, Profile, Organiser
│       ├── Filters/                # SessionAuthorize (login + role enforcement)
│       ├── Services/               # ApiClient (HTTP only), ApiRoutes, session helpers
│       ├── Models/                 # API DTOs + view models
│       ├── Helpers/                # formatting (ordinals), upload validation
│       ├── Views/                  # Razor views (role-aware layout)
│       └── wwwroot/css/site.css    # RaceDay brand theme
└── tests/RaceDay.Web.Tests/        # xUnit tests
```

## Run locally (VS Code)

Prerequisites: [.NET 8 SDK](https://dotnet.microsoft.com/download), VS Code + C# Dev Kit, and the Part 2 API running.

```bash
# Start the separately supplied API first (note its URL, e.g. http://localhost:5000).
# Configure Api:BaseUrl in src/RaceDay.Web/appsettings.json or Api__BaseUrl environment variable.
dotnet run --project src/RaceDay.Web
# open http://localhost:5080
```

## Run with Docker

```bash
docker build -t raceday-web .
docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:5000/ raceday-web
# open http://localhost:8080
```

* `Api__BaseUrl` must be reachable **from inside the container** (`host.docker.internal` reaches an API running on
  your machine; on Linux add `--add-host=host.docker.internal:host-gateway`). If the API is deployed (e.g. Azure App Service)
  use its public URL.
* Behaviour is identical to local runs – the only difference is the configured API URL.

## Tests

```bash
dotnet test RaceDay.sln
```

Covers: API client (auth header, error mapping, uploads), session login/logout, redirect safety, role enforcement
(filter + controller attributes), participant/organiser controller flows, result-entry validation, upload validation.

## Architecture

```
Browser ⇄ MVC (this repo) ──HTTP + JWT──▶ Part 2 API ──▶ Database
                                              └────────▶ Azure Blob Storage
```

* **Auth/session** – `AccountController` posts credentials to the API; the returned JWT, user id, name and role are stored
  in server-side session (HttpOnly cookie holds only the session id). Logout clears the session and redirects to login.
* **Access control** – `[SessionAuthorize(Role = "...")]` redirects anonymous users to login and wrong-role users to *Access denied*.
  Anti-forgery validation is applied to every POST.
* **Blob Storage** – images are uploaded as multipart to the API (`/banner`, `/picture`); the API stores them in Azure Blob,
  saves the URL, and the MVC views render that URL. The MVC app never talks to Azure directly.

## API plan alignment

The client route helpers in `src/RaceDay.Web/Services/ApiRoutes.cs` follow the supplied `API PLAN.pdf`, Section B.
The route-by-route mapping, request field alignment, plan ambiguities, and optional extensions are documented in
[`docs/API-PLAN-ALIGNMENT.md`](docs/API-PLAN-ALIGNMENT.md). Use [`docs/API-VERIFICATION-CHECKLIST.md`](docs/API-VERIFICATION-CHECKLIST.md)
to test the actual API in Swagger.

Important contract points:
- Registration sends `FirstName`, `LastName`, `Email`, and `Password`; the plan does not list a role field.
- Own profile uses `/api/users/me`.
- Participant enrolments use `/api/enrolments`; event entry posts to `/api/events/{eventID}/enrolments`.
- Result creation is `/api/enrolments/{enrolmentID}/results`; participant results are `/api/users/me/results`.
- Category update/delete use `/api/categories/{categoryID}`.

The plan does not define upload or publish-result endpoints, so those controls were removed from the plan-aligned MVC UI.

## Part 2 feedback remediation checklist

- [x] README with structure, Docker commands, CI + video placeholders
- [x] CI workflow builds the MVC app, runs tests, and builds the Docker image (the API project is external)
- [x] Expanded tests: authentication, resource endpoints, role enforcement
- [ ] Push the included local commit history to your GitHub repository (20 commits are included in this archive)
- [ ] CI screenshot + YouTube link added above
