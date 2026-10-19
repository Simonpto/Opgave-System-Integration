# 2. Repository-struktur

```
apps/userprofile-microservice/   ← selve servicen (src, tests, Dockerfile, README)
packages/Shared.Contracts/       ← fælles events, som alle services deler
packages/MessageClient/          ← wrapper om RabbitMQ
docs/                            ← denne dokumentation
```

- **`apps/`** indeholder kode til én bestemt service.
- **`packages/`** indeholder kode, som flere services kan genbruge. Packages må aldrig kende til en app.
