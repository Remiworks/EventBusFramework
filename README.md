[<img src="https://mikederksen5.visualstudio.com/_apis/public/build/definitions/806ded94-c145-49ce-a757-6d5323d31e66/1/badge"/>](https://mikederksen5.visualstudio.com/EventBusFramework/_build/index?definitionId=1)

- [Introduction](#introduction)
- [Usage](#usage)
    - [Adding Remiworks to your project](#adding-remiworks-to-your-project)
        - [Project types](#project-types)
            - [Console project](#console-project)
            - [MVC project](#mvc-project)
        - [Logging](#logging)
    - [Events](#events)
        - [Sending events](#sending-events)
        - [Receiving events with the IEventListener](#receiving-events-with-the-ieventlistener)
        - [Receiving events with Remiworks.Attributes](#receiving-events-with-remiworksattributes)
    - [Commands](#commands)
        - [Sending commands](#sending-commands)
        - [Receiving events with the IEventListener](#receiving-events-with-the-ieventlistener)
        - [Receiving events with Remiworks.Attributes](#receiving-events-with-remiworksattributes)
- [Example projects](#example-projects)

# Introduction
This framework is an abstraction layer for eventbusses. As of now the only implementation is for RabbitMq. There are plans to support more eventbusses in the future.

# Usage
The core library contains multiple classes to communicate with an eventbus implementation of choice.

## Adding Remiworks to your project
Before Remiworks can be used, it needs to be added to your project. This is done via a dependency injection mechanism. This can be be both a simple console project or an more sophisticated MVC projects (which also is a console application).

### Project types
#### Console project
Add a reference to `Microsoft.Extensions.DependencyInjection`. 

**Important:** In case of a controller which only listens to commands or events, it is important to still call `serviceProvider.GetService<...>();`. By doing this, you let the dependency injection mechanism create an instance of the desired class. This in turn, instantiates any listeners which enables the class to listen to incoming events/commands.

```csharp
var serviceProvider = new ServiceCollection()
    .AddTransient<SomeRandomClass>() // Add your dependency stuff here
    .AddRabbitMq(new BusOptions()) // Use an RabbitMq implementation (Remiworks.RabbitMQ)
    .BuildServiceProvider();

// Option 1:
var randomClass = serviceProvider.GetService<SomeRandomClass>();
randomClass.DoSomething(); 
// This class will have the required dependencies injection together with required Remiworks classes

// Option 2:
serviceProvider.UseAttributes(); // Remiworks.Attributes
```

#### MVC project
Adding Remiworks to an MVC project is similar to adding it to a console project.

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddTransient<SomeRandomClass>()
        services.AddRabbitMq(new BusOptions());
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseBrowserLink();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();

        // Option 1:
        serviceProvider.GetService<SomeRandomClass>(); // Let the class be instantiated

        // Option 2:
        app.ApplicationServices.UseAttributes();

        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "default",
                template: "{controller=RDW}/{action=Index}");
        });
    }
}
```

### Logging
Remiworks also allows for a logger to be added. This is done as an extra parameter in the `AddRabbitMQ` method. The following example illustrates how `Serilog` can be added to the mix:

```csharp
var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var loggerFactory = new LoggerFactory()
    .AddSerilog(logger);

services.AddRabbitMq(new BusOptions(), loggerFactory);
```

## Events
Events are the core mechanism of web scale architecture. Remiworks provides methods for both sending and receiving events.

### Sending events
First, add `Remiworks.Core.Event.Publisher.IEventPublisher` to the constructor of the desired class. Secondly, define a method which sends an event. This event is just a simple POCO.
```csharp
public class SomeRandomClass
{
    private readonly IEventPublisher _eventPublisher;

    public SomeRandomClass(IEventPublisher eventPublisher) 
    {
        _eventPublisher = eventPublisher;
    }

    public async Task SendSomeEventMessage(SomeEvent someEvent) 
    {
        await _eventPublisher.SendEventAsync(
            message: someEvent, 
            routingKey: "some.event.sent");
    }
}
```

### Receiving events with the IEventListener
Receiving events is very similar to sending them. Again, create a constructor. Add `Remiworks.Core.Event.Listener.IEventListener` to the constructor this time. Also define a method which will be called when the `some.event.sent` message comes in.
```csharp
public class SomeRandomClass
{
    public SomeRandomClass(IEventListener eventListener) 
    {
        eventListener
            .SetupQueueListenerAsync<SomeEvent>(
                queueName: "someEventQueue", 
                topic: "some.event.sent", 
                callback: SomeEventReceived)
            .Wait();
    }

    private void SomeEventReceived(SomeEvent receivedEvent) 
    {
        // Do something to handle the event
    }
}
```

### Receiving events with Remiworks.Attributes
Events can also be received by using the `Remiworks.Attributes` library. This allowes you to use attributes for event receiving. This can be done as follows:

First, add the attributes library to the dependency injection mechanism
```csharp
var serviceProvider = new ServiceCollection()
    .AddTransient<SomeRandomClass>() // Add your dependency stuff here
    .AddRabbitMq(new BusOptions()) // Use an RabbitMq implementation (Remiworks.RabbitMQ)
    .BuildServiceProvider();

serviceProvider.UseAttributes();
```

Secondly, add the correct attributes to the desired class
```csharp
[QueueListener("someEventQueue")]
public class SomeRandomClass
{
    [Event("some.event.sent")]
    private void SomeEventReceived(SomeEvent receivedEvent) 
    {
        // Do something to handle the event
    }
}
```

## Commands
Sometimes, events are not enough. There are situations where a callback is desired. Commands are a good solution to this problem.

### Sending commands
Sending commands is very similar to sending events.

First, let `Remiworks.Core.Command.Publisher.ICommandPublisher` be injected in the constructor of the desired class.
```csharp
public class SomeRandomClass
{
    private readonly ICommandPublisher _commandPublisher;

    public SomeRandomClass(ICommandPublisher commandPublisher) 
    {
        _commandPublisher = commandPublisher;
    }
}
```

Secondly, define a method which publishes a command. This command waits for a response from the server
```csharp
public async Task SendSomeCommand(SomeCommand command)
{
    return _commandPublisher.SendCommandAsync(
        message: command,
        queueName: "someCommandQueue",
        key: "some.command.sent");
}
```

Alternatively, a command can expect a result from the server.
```csharp
public async Task<int> CalculateSomethingOnServer(SomeCommand command) 
{
    return await _commandPublisher.SendCommand<int>(
        message: command,
        queueName: "someCommandQueue",
        key: "some.calculation.sent");
}
``` 

### Receiving events with the IEventListener
First, add a constructor to the desired class which injects `Remiworks.Core.Command.Listener.ICommandListener`

Secondly, add callbacks for the command without and with a return value. Please note: Right now, using `Task` or `void` as the return type is not supported. Returning null results in the command to be handled correctly on the sending end when used as void. This will be fixed in an future patch.

```csharp
public class SomeRandomClass
{
    public SomeRandomClass(ICommandListener commandListener) 
    {
        commandListener.SetupCommandListenerAsync<SomeCommand>(
            queueName: "someCommandQueue",
            key: "some.command.sent",
            callback: HandleVoidCommand);

        commandListener.SetupCommandListenerAsync<SomeCommand>(
            queueName: "someCommandQueue",
            key: "some.calculation.sent",
            callback: HandleCalculateCommand);
    }

    private Task<object> HandleVoidCommand(SomeCommand command)
    {
        // Do something with the command

        return null;
    }

    private Task<object> HandleCalculateCommand(SomeCommand command)
    {
        // Imagine command has a Value property
        return Task.FromResult<object>(command.Value * 10); 
    }
}
```

### Receiving events with Remiworks.Attributes
Commands can also be received by using the `Remiworks.Attributes` library. This allowes you to use attributes for command receiving. This can be done as follows:

First, add the attributes library to the dependency injection mechanism
```csharp
var serviceProvider = new ServiceCollection()
    .AddTransient<SomeRandomClass>() // Add your dependency stuff here
    .AddRabbitMq(new BusOptions()) // Use an RabbitMq implementation (Remiworks.RabbitMQ)
    .BuildServiceProvider();

serviceProvider.UseAttributes();
```

Secondly, add attributes to the desired class
```csharp
[QueueListener("someCommandQueue")]
public class SomeRandomClass
{
    [Command("some.command.sent")]
    private void SomeEventReceived(SomeEvent receivedEvent) 
    {
        // Do something to handle the command
    }

    [Command("some.calculation.sent")]
    public int HandleCalculateCommand(SomeCommand command)
    {
        // Imagine that SomeCommand has a Value property
        return 10 * command.Value
    }
}
```

# Example projects
There are some example projects set up. These can be found in the `Examples` folder in this repository. Here are example projects defined for:
- Sending events: `EventPublishExample`
- Listening to events: `EventListenExample`
- Sending commands: `RpcTest`
- Listening to commands: `RpcServerTest`