# RaceDay Part 3 — Submission audit (updated with Part 2 API)

This audit compares the archive contents with the supplied Part 3 rubric. It records evidence in the ZIP separately from checks that require a live environment.

| Rubric area | Archive evidence | Status |
|---|---|---|
| Correct submission (5) | MVC, API, solution, tests, README, Dockerfiles and docs included | Present; final external evidence outstanding |
| GitHub and CI/CD (10) | 20 local commits existed before integration; workflow now builds both target frameworks, tests and both images | Configured; remote push and green run not verified |
| Auth and navigation (10) | MVC login/register/session/role navigation; API source included | Partial: API login issues cookie, MVC expects bearer token; `/api/...` routes mismatch API `[controller]` routes |
| Organiser interface (15) | Dashboard, events, categories, enrolments and results MVC pages/controllers | UI present; end-to-end API route/role behavior requires verification |
| Participant interface (15) | Catalogue/details, enrolments, results and profile MVC pages/controllers | UI present; user-scoped API operations/profile route require implementation/verification |
| Azure Blob (10) | MVC upload client helper exists | Not complete: supplied API has no Blob upload endpoints or image URL persistence |
| Docker (10) | MVC Dockerfile and API Dockerfile; CI builds both images | Image build configured; runtime needs reachable SQL Server and live validation |
| UI design (10) | RaceDay CSS, Bootstrap-oriented Razor views and status components | Source present; responsive visual QA still required |
| Video (15) | README contains demonstration scope | Outstanding: unlisted video URL and recording required |

## Critical technical blockers identified in the supplied Part 2 API

1. `RaceDay.Api` targets .NET 10 while the MVC project targets .NET 8; CI therefore installs both SDKs.
2. API controllers use `[Route("[controller]")]`, so the actual routes are `/Auth`, `/Events`, `/Users`, etc. The MVC client expects `/api/auth`, `/api/events`, `/api/users/me`, and related route shapes.
3. API login authenticates with a cookie and returns `{ UserId, Email, Role }`; the MVC login expects a bearer `Token` and `FullName`. The two authentication contracts are not compatible as supplied.
4. The API does not expose all MVC-required operations, notably own-profile (`/api/users/me`), user-scoped results/enrolments, event-scoped category/enrolment routes, and some result-entry routes.
5. The supplied API has no Azure Blob Storage service/controller endpoints and its User/Event models do not persist profile/banner image URLs.
6. The API requires a SQL Server connection string and an existing database schema. The archive does not include a database backup/migrations or live connection credentials.

## Before submission

- Align the API route and authentication contracts with `src/RaceDay.Web/Services/ApiRoutes.cs`, `ApiClient.cs`, and `AccountController.cs`.
- Implement and test Azure Blob upload endpoints in the API, including persisted URLs and MVC display.
- Verify database connection, role enforcement, organiser CRUD, participant entry, results, and profile updates against a real database.
- Push to GitHub; confirm 20+ meaningful commits in the remote repository and a green Actions run.
- Add the CI screenshot and unlisted demonstration video URL to README.
