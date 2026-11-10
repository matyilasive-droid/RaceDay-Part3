# RaceDay – Part 3 (ASP.NET Core MVC front end)

An ASP.NET Core 8 MVC web app for the RaceDay event platform. It consumes the **Part 2 REST API** for every
data operation. There is **no DbContext, no database connection and no Azure SDK** in this project.

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
# 1. start the Part 2 API (note its URL, e.g. http://localhost:5000)
# 2. point the MVC app at it – edit src/RaceDay.Web/appsettings.json  ("Api": { "BaseUrl": ... })
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

## Part 2 API contract expected by the MVC app

All paths live in `src/RaceDay.Web/Services/ApiRoutes.cs`; DTO shapes in `Models/ApiModels.cs` (JSON is camelCase).
**If your Part 2 API differs, change those two files only.**

| Purpose | Method & path | Notes |
|---|---|---|
| Register | `POST api/auth/register` | `{fullName,email,password,role}` |
| Login | `POST api/auth/login` | → `{token,userId,fullName,email,role,profileImageUrl}` |
| Events | `GET api/events`, `GET api/events/{id}` | list should include `categories` |
| Organiser events | `GET api/events/mine` | includes `enrolmentCount` |
| Event CRUD | `POST api/events`, `PUT/DELETE api/events/{id}` | |
| Banner | `POST api/events/{id}/banner` (multipart `file`) | → `{url}` |
| Categories | `GET/POST api/events/{id}/categories`, `DELETE .../categories/{cid}` | `{name,type}` |
| Enter event | `POST api/events/{id}/enrol` | `{categoryId}` |
| Event enrolments | `GET api/events/{id}/enrolments` | |
| My enrolments | `GET api/enrolments/mine` | status Pending / Confirmed / Cancelled |
| Results | `GET/POST api/events/{id}/results`, `POST .../results/publish` | `{enrolmentId,finishTime("hh:mm:ss"),position}` |
| My results | `GET api/results/mine` | includes `position`, `totalFinishers` |
| Profile | `GET/PUT api/profile`, `POST api/profile/picture` (multipart `file`) | |

## Part 2 feedback remediation checklist

- [x] README with structure, Docker commands, CI + video placeholders
- [x] CI workflow builds API + MVC, runs tests, builds Docker image
- [x] Expanded tests: authentication, resource endpoints, role enforcement
- [ ] 20+ meaningful commits pushed (see your git history)
- [ ] CI screenshot + YouTube link added above
