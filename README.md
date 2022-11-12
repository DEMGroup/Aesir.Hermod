# Aesir.Hermod
*Hermóðr (Hermod) - Messenger to the Norse gods.*

![Hermod](https://user-images.githubusercontent.com/11881500/201449784-c2cf3c06-e2a5-46bf-99f3-df56dfb4353a.png)

Hermod is a wrapper written for RabbitMQ to facilitate messaging on the Aesir platform.

##  Getting Started
All configuration for Hermod happens when registering it within the dependency injection container:
```csharp
// Where services is IServiceCollection
services.AddHermod();
```
##### Add connection info
```csharp
services.AddHermod(
	conf.ConfigureHost(opts =>
            {
                opts.Host = "localhost";
                opts.Port = 5672;
                opts.VHost = "/";
                opts.User = "guest";
                opts.Pass = "guest";
            })
);
```

##### Add queue consumer
```csharp
services.AddHermod(
	conf.ConsumeQueue("test-queue", x =>
            {
                x.RegisterConsumer(typeof(SampleMessageConsumer));
            });
);
```

##### Add exchange consumer
 ```csharp
services.AddHermod(
	conf.ConsumeExchange("test-exchange", x =>
            {
                x.RegisterConsumer(typeof(SampleMessageConsumer));
            });
);
```
