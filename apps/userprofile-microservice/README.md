# UserProfile microservice

Stores user profiles (username, display name, email, bio, avatar) and counts how many messages each user has posted.

## Endpoints

| Method | Route | Result |
|---|---|---|
| POST | `/api/userprofiles` | 201, 400, 409 (username taken) |
| GET | `/api/userprofiles` | 200 |
| GET | `/api/userprofiles/{id}` | 200, 404 |
| PUT | `/api/userprofiles/{id}` | 200, 400, 404 |
| DELETE | `/api/userprofiles/{id}` | 204, 404 |

## Run

```bash
docker compose up --build
```

- API: http://localhost:8000/api/userprofiles
- RabbitMQ UI: http://localhost:15672 (guest/guest)

## Test

```bash
dotnet test UserProfile.sln
```

Docker must be running: the RabbitMQ integration tests start their own broker with Testcontainers.

## Events

**Consumes**
- `MessagePostedEvent`: increments the author's message count.

**Publishes**
- `UserActivityRecordedEvent`: after each `MessagePostedEvent` (`MessageId`, `UserId`, `MessageCount`, `ProcessedAt`).
- `UserProfileCreatedEvent`, `UserProfileUpdatedEvent`, `UserProfileDeletedEvent`: after each write.

The contracts live in `packages/Shared.Contracts`.
