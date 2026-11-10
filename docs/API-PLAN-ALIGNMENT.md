# API plan alignment — RaceDay Part 3

This document records how the MVC front end is expected to call the supplied **API PLAN.pdf, Section B**.
The plan is the source of truth; the MVC app does not implement a database or the REST API.

## Aligned routes

| Feature | HTTP method | API plan route | MVC usage |
|---|---|---|---|
| Register | POST | `/api/auth/register` | `AccountController.Register` |
| Login | POST | `/api/auth/login` | `AccountController.Login` |
| Own profile | GET / PUT | `/api/users/me` | `ProfileController.Index` |
| Change password | PUT | `/api/users/me/password` | route constant available |
| User details | GET | `/api/users/{userID}` | route helper available |
| User list | GET | `/api/user` | route constant available |
| Events | GET / GET by id | `/api/events`, `/api/events/{eventID}` | home and event details |
| Event CRUD | POST / PUT / DELETE | `/api/events`, `/api/events/{eventID}` | organiser controller |
| Event enrolments | GET | `/api/events/{eventID}/enrolments` | organiser enrolments |
| Event categories | GET / POST | `/api/events/{eventID}/categories` | event details / organiser categories |
| Category details and CRUD | GET / PUT / DELETE | `/api/categories/{categoryID}` | route helpers available |
| Participant enrolments | POST | `/api/events/{eventID}/enrolments` | event entry |
| Own enrolments | GET | `/api/enrolments` | participant enrolments |
| Enrolment details / update / cancel | GET / PUT / DELETE | `/api/enrolments/{enrolmentID}` | route helper available |
| Enrolment result | POST | `/api/enrolments/{enrolmentID}/results` | organiser result entry |
| Result details / update / delete | GET / PUT / DELETE | `/api/results/{resultID}` | route helper available |
| Result for enrolment | GET | `/api/enrolments/{enrolmentID}/result` | route helper available |
| Event results | GET | `/api/events/{eventID}/results` | organiser results |
| Participant results | GET | `/api/users/me/results` | participant results |
| Event types | GET / GET by id / POST / PUT / DELETE | `/api/event-types`, `/api/event-types/{eventTypesID}` | route helpers available |

## Important contract corrections

- Registration sends `FirstName`, `LastName`, `Email`, and `Password`. The supplied plan does not list `Role` in the registration body, so the registration screen no longer sends a role selector. Organiser permissions must be assigned by an administrator or by the API's own policy.
- The profile route is `/api/users/me`, not `/api/profile`.
- Participant enrolment uses the nested `/enrolments` route and sends `CategoryID`.
- Result creation is attached to an enrolment: `/api/enrolments/{enrolmentID}/results`, with `FinishTime` and `Position`.
- Participant results use `/api/users/me/results`, not `/api/results/mine`.
- Category update/delete uses `/api/categories/{categoryID}`; it is not nested under the event route.

## Plan ambiguities that must be confirmed against the API implementation

The supplied plan uses both `/api/events/{eventID}/enrolments` and `/api/event/{eventID}/enrolments` for event enrolment listing. The MVC client uses the plural `/api/events/...` version because that is also used for event resources elsewhere.

The plan's event-type POST body describes event-like fields (`EventName`, `EventType`, `EventDate`, `Venue`, `City`, `StartTime`, `Status`). This document preserves the plan's route and does not reinterpret that payload.

The plan does not specify JSON response property casing or exact DTO envelopes. The MVC client expects ordinary JSON objects/arrays and camelCase-compatible properties. Confirm actual API serialization before deployment.

## Features intentionally removed from the plan-aligned UI

The MVC front end no longer calls banner upload, profile-picture upload, organiser-only `/events/mine`, or result-publication endpoints because none of those routes are defined in the supplied plan. The organiser dashboard now loads the public event list and filters it by the signed-in organiser ID returned with each event.

## Verification status

This is a front-end contract alignment, not proof that the remote API exists or that its database operations work. Run the MVC tests and exercise every endpoint against the actual API/Swagger instance before submission. Record status codes, valid/invalid requests, missing IDs, role failures, and persistence evidence.

## Full Section B endpoint and role matrix

| Method | Route | Required role | Request body from plan |
|---|---|---|---|
| POST | `/api/auth/register` | Public | `FirstName`, `LastName`, `Email`, `Password` |
| POST | `/api/auth/login` | Public | `Email`, `Password` |
| GET | `/api/users/me` | Authenticated | None |
| PUT | `/api/users/me` | Authenticated | `FirstName`, `LastName`, `Email` |
| PUT | `/api/users/me/password` | Authenticated | `currentPassword`, `newPassword`, `confirmPassword` |
| GET | `/api/users/{userID}` | Admin | None |
| GET | `/api/user` | Admin | None |
| GET | `/api/events` | Public | None |
| GET | `/api/events/{eventID}` | Public | None |
| POST | `/api/events` | Admin, Organiser | `VenueID`, `Name`, `description`, `eventDate` |
| PUT | `/api/events/{eventID}` | Admin, Organiser | `VenueID`, `Name`, `Description`, `eventDate` |
| DELETE | `/api/events/{eventID}` | Admin, Organiser | None |
| GET | `/api/events/{eventID}/enrolments` | Admin, Organiser | None |
| GET | `/api/events/{eventID}/categories` | Public | None |
| GET | `/api/categories/{CategoryID}` | Public | None |
| POST | `/api/events/{EventID}/categories` | Admin, Organiser | `Name`, `DistanceKm`, `MaxParticipants`, `EntryFee` |
| PUT | `/api/categories/{CategoryID}` | Admin, Organiser | `Name`, `DistanceKm`, `MaxParticipants`, `EntryFee` |
| DELETE | `/api/categories/{CategoryID}` | Admin, Organiser | None |
| POST | `/api/events/{EventID}/enrolments` | Participant | `CategoryID` |
| GET | `/api/enrolments` | Participant | None |
| GET | `/api/enrolments/{EnrolmentID}` | Participant, Admin, Organiser | None |
| PUT | `/api/enrolments/{EnrolmentID}` | Admin, Organiser | `Status` |
| DELETE | `/api/enrolments/{EnrolmentID}` | Participant, Admin, Organiser | None |
| GET | `/api/event/{EventID}/enrolments` | Admin, Organiser | None (singular `/event` in plan) |
| POST | `/api/enrolments/{EnrolmentID}/results` | Admin, Organiser | `FinishTime`, `Position` |
| GET | `/api/results/{ResultID}` | Authenticated | None |
| PUT | `/api/results/{ResultID}` | Admin, Organiser | `FinishTime`, `Position` |
| DELETE | `/api/results/{ResultID}` | Admin | None |
| GET | `/api/enrolments/{EnrolmentID}/result` | Participant, Admin, Organiser | None |
| GET | `/api/events/{eventId}/results` | Admin, Organiser | None |
| GET | `/api/users/me/results` | Participant | None |
| GET | `/api/event-types` | Public | None |
| GET | `/api/event-types/{eventTypesID}` | Public | None |
| POST | `/api/event-types` | Admin | `EventName`, `EventType`, `Description`, `EventDate`, `Venue`, `City`, `StartTime`, `Status` |
| PUT | `/api/event-types/{eventTypesID}` | Admin | Same event-type fields as POST |
| DELETE | `/api/event-types/{eventTypesID}` | Admin | None |

The route helper exposes the complete resource route set. The current MVC screens call only the subset needed by their workflows; endpoints without a corresponding screen remain documented and can be exercised directly in Swagger. The API itself must enforce every role in this table—the MVC session filter is only an additional presentation-layer guard.
