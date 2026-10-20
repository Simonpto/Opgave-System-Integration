# 7. Packages

- **`Shared.Contracts`:** De events og DTO'er, services sender til hinanden. Navnene må ikke ændres, fordi de
  bestemmer RabbitMQ-exchangens navn.
- **`MessageClient`:** `IMessageClient` skjuler RabbitMQ og EasyNetQ.
  `AddMessageClient()` sætter forbindelsen op, og `AddMessageHandler<TEvent, THandler>()` registrerer en consumer.
- **`MessageClient.TestHelpers`:** `FakeMessageClient`, så man kan teste uden RabbitMQ.

En ny microservice kan genbruge de samme packages.
