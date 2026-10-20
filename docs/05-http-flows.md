# 5. HTTP-flows

| Metode | Route | Svar |
|---|---|---|
| POST | `/api/userprofiles` | 201, 400, 409 (brugernavn taget) |
| GET | `/api/userprofiles` | 200 |
| GET | `/api/userprofiles/{id}` | 200, 404 |
| PUT | `/api/userprofiles/{id}` | 200, 400, 404 |
| DELETE | `/api/userprofiles/{id}` | 204, 404 |

**Flow:** Controller → `UserProfileService` → Domain (validering) → Repository → event til RabbitMQ.

Exceptions bliver til statuskoder ét sted (`ExceptionToProblemDetailsHandler`):
validering giver 400, ikke fundet giver 404, og konflikt giver 409.
