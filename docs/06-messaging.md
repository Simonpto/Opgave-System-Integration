# 6. Messaging (RabbitMQ)

| Event | Retning | Hvornår |
|---|---|---|
| `MessagePostedEvent` | Ind | En bruger har skrevet en besked → tælleren tælles op |
| `UserActivityRecordedEvent` | Ud | Efter hver behandlet besked (indeholder den nye tæller) |
| `UserProfileCreated/Updated/DeletedEvent` | Ud | Efter POST, PUT og DELETE |

**Sådan modtages beskeder:** Ved opstart abonnerer servicen én gang. Derefter sender RabbitMQ beskederne til den.
Hver besked får en ny `MessagePostedHandler`. Lykkes den, bliver beskeden slettet fra køen (ACK).
Fejler den, havner beskeden i fejlkøen.

**Køer:** EasyNetQ laver én exchange pr. eventtype og én kø pr. abonnent. Servicer med forskellige navne får hver
deres kopi af hver besked.

**Konfiguration:** `RabbitMQ:Host` læses fra `appsettings.json` (`localhost`) og overskrives i Docker af
environment variablen `RabbitMQ__Host=rabbitmq`.
