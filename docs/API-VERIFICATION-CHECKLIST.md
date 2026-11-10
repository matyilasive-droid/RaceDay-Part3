# API verification checklist

Use the API's Swagger page and a clean test database. Mark an item only after observing the actual response.

- [ ] Register: valid account returns 201; duplicate email / invalid body returns validation error; verify password is hashed in storage.
- [ ] Login: valid credentials return 200 and token; invalid credentials are rejected.
- [ ] `GET /api/users/me`: anonymous request rejected; authenticated request returns own profile.
- [ ] `PUT /api/users/me`: valid update persists; invalid email / duplicate email rejected.
- [ ] `PUT /api/users/me/password`: wrong current password rejected; valid change works; old password no longer works.
- [ ] Admin-only user detail/list endpoints reject non-admin users.
- [ ] Events list/detail are public; event create/update/delete enforce Admin or Organiser.
- [ ] Event category list/detail are public; category create/update/delete enforce Admin or Organiser.
- [ ] Event enrolment creation is Participant-only; duplicate enrolment and invalid category are rejected.
- [ ] Own enrolments and enrolment details enforce participant ownership; organiser/admin access follows the plan.
- [ ] Enrolment status update enforces Admin or Organiser; cancellation enforces the listed roles.
- [ ] Result create/update/delete enforce the roles in the plan; result lookups return the correct enrolment/event.
- [ ] Event types list/detail are public; create/update/delete enforce Admin.
- [ ] Test missing IDs, malformed bodies, FK/unique constraints, and persistence after restart.
- [ ] Capture Swagger request/response evidence and include it in the submission.

Do not mark a check complete based only on a successful MVC page render; verify the API response and database effect.
