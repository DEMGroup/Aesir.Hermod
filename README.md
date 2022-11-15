
# Aesir.Hermod
<div align="center">
	<img height="200" src="https://user-images.githubusercontent.com/11881500/201449784-c2cf3c06-e2a5-46bf-99f3-df56dfb4353a.png"/>
</div>

<div align="center">
	<i>Hermóðr (Hermod) - Messenger to the Norse gods.</i>
</div>
<br/>

Hermod is a wrapper written for RabbitMQ to facilitate messaging on the Aesir platform.

##  Getting Started
All configuration for Hermod happens when registering it within the dependency injection container:
```csharp
// Where services is IServiceCollection
services.AddHermod();
```

#### Example config

```csharp
services.AddHermod(builder => 
		builder.ConfigureHost(opts =>
		{
			opts.Host = "localhost";
			opts.Port = 5672;
			opts.VHost = "/";
			opts.User = "guest";
			opts.Pass = "guest";
		});
	
		builder.ConsumeQueue("queue-1", conf =>
		{
			conf.RegisterConsumer(typeof(SampleMessageConsumer));
			conf.RegisterConsumer(typeof(StateMessageConsumer));
		});
	
		builder.ConsumeQueue("queue-2", x =>
		{
			conf.RegisterConsumer(typeof(SampleMessageConsumer));
			conf.RegisterConsumer(typeof(StateMessageConsumer));
		});
			
		builder.ConsumeExchange("exchange-1", x => 
		{
			conf.RegisterConsumer(typeof(StateMessageConsumer));
		});

		builder.ConfigureProducer(conf =>
                {
                    conf.ResponseTimeout = TimeSpan.FromSeconds(30);
                });
	);
```

##### Add connection info
```csharp
services.AddHermod(builder => 
	builder.ConfigureHost(opts =>
	{
		opts.Host = "localhost";
		opts.Port = 5672;
		opts.VHost = "/";
		opts.User = "guest";
		opts.Pass = "guest";
	});
);
```

##### Adding a queue consumer
```csharp
services.AddHermod(builder => 
	builder.ConsumeQueue("test-queue", x =>
        {
        	x.RegisterConsumer(typeof(SampleMessageConsumer));
        });
);
```

##### Adding an exchange consumer
 ```csharp
services.AddHermod(builder => 
	builder.ConsumeExchange("test-exchange", x =>
        {
        	x.RegisterConsumer(typeof(SampleMessageConsumer));
        });
);
```
