# 8. Deployment og test

**Kør:** `docker compose up --build` i `apps/userprofile-microservice/`
→ API på http://localhost:8000/api/userprofiles, RabbitMQ UI på http://localhost:15672 (guest/guest).

**Compose:** Servicen starter først, når RabbitMQ er klar. Det styres af et healthcheck med
`rabbitmq-diagnostics check_port_connectivity` og `depends_on: condition: service_healthy`.

**Test:** `dotnet test UserProfile.sln`
- Unit-tests af domænet og handleren.
- Integrationstests af API'et og af rigtig RabbitMQ via Testcontainers. De kræver Docker.

**CI:** GitHub Actions kører alle tests ved push og pull requests.
