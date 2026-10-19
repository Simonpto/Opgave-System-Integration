# 9. Begrænsninger og roadmap

**Kendte svagheder**
- Data ligger i hukommelsen og forsvinder ved genstart.
- Leveres den samme besked to gange, bliver den talt to gange.
- Events kan gå tabt, hvis RabbitMQ er nede (intet outbox-pattern).
- Der er ingen authentication.

**Næste skridt**
1. MSSQL med EF Core.
2. Relationship-service, der lytter på profil-events.
3. Avatar-service med upload af billeder.
4. API Gateway.
