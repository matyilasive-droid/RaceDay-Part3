# API verification checklist

Run the Part 2 API with a valid SQL Server connection string and open its Swagger page in Development.

## Required integration checks

- [ ] Confirm API base URL and HTTPS certificate behavior from MVC.
- [ ] Confirm registration request/response matches MVC `Register` flow.
- [ ] Confirm login returns the token/user fields expected by MVC or update both sides to a single secure auth contract.
- [ ] Confirm MVC bearer token is accepted by every protected API endpoint.
- [ ] Confirm API route templates match `ApiRoutes.cs` exactly.
- [ ] Confirm `/api/users/me` reads/updates only the authenticated user's own profile.
- [ ] Confirm enrolment list and results are scoped to the authenticated participant.
- [ ] Confirm event categories/enrolments/results are correctly scoped to the selected event.
- [ ] Confirm Organiser-only endpoints reject Participants and unauthenticated requests.
- [ ] Confirm event banner and profile-picture multipart uploads pass through the API to Azure Blob Storage and persist returned URLs.
- [ ] Confirm MVC displays stored banner/profile image URLs.
- [ ] Confirm Docker containers can reach SQL Server and each other.

The supplied Part 2 source does not currently satisfy all these contract checks. Mark each item only after testing against the running API and database.
