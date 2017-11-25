This framework abstracts the communication to an eventbus. Right now the only implemented eventbus is RabbitMQ. There are plans to support more eventbus implementations in the future.

# Core
The core library contains multiple classes to communicate with an eventbus implementation

## Events
To send and receive events there is the `IBusProvider` interface with a corresponding `RabbitBusProvider` class. This class can be used to send and listen to events. The `RabbitBusProvider` takes a `BusOptions` instance as parameter. This class contains connection data. The `Hostname`, `ExchangeName`, `VirtualHost`, `Port`, `UserName` and `Password` can be set here. By default the `BusOptions` uses `localhost` as `Hostname`. `BusOptions` also declases a random exchange name. If the other properties are not set, the default values of the underlying eventbus framework will be used.

```csharp
BusOptions options = new BusOptions();
IBusProvider busProvider = new RabbitBusProvider(options);
```

### Sending events
For sending evens we can use `void BasicPublish(EventMessage eventMessage)`. This is a low level function which takes, as we can see, an `EventMessage` object as parameter. The `EventMessage` object contains data such as `RoutingKey`, `CorrelationId`, `Timestamp`, `ReplyQueueName`, `Type` and a `JsonMessage`. For sending plain events only the `RoutingKey` and the `JsonMessage` should be set.

Event routing is handled by an exchange. The exchange will determine, based on the routing key, to which queue the event will go. To register this route we can use `void CreateTopicsForQueue(string queueName, params string[] topics)`. This method is idempotent. THis means that calling this function multiple times results in the exact same behaviour. Topics can contain the wildcards `*` and `#`.
- `*` means that this position can contain any word
- `#` means that this positoin can contain multiple words separated by dots

**Note:** In a next version of this framework (which will come soon) we use a higher level class for sending and listening to events. This will be done by using a `EventProvider` class and a `EventListener` class. This way we won't have to deal with low-level `EventMessage` objects.

```csharp
public class PersonCreatedEvent
{
    public string Name { get; set; }
}
```

```csharp
// Matches on "foo.something.bar"
// Doesn't match on "foo.something.else.bar"
busProvider.CreateTopicsForQueue("SomeRandomQueue", "foo.*.bar");

// Matches on "test.something.toost"
// Matches on "test.something.else.toost"
busProvider.CreateTopicsForQueue("AnotherRandomQueue", "test.#.toost");

PersonCreatedEvent personEvent = new PersonCreatedEvent
{
    Name = "Jan"
};

// Newtonsoft.JSON functionality
string personEventJson = JsonConvert.SerializeObject(personEvent);

EventMessage message = new EventMessage
{
    // Matches on the first topic declaration, meaning this event will be published to the queue `SomeRandomQueue`
    RoutingKey = "foo.bla.bar",
    JsonMessage = personEventJson
};

busProvider.BasicPublish(message);
```

Wrapper for RabbitMQ. This includes an attribute library for handling incoming events and commands.

### Receiving events
Events can be received by using the `void BasicConsume(string queueName, EventReceivedCallback callback);` function. The `EventReceivedCallback` is an delagate:

```csharp
public delegate void EventReceivedCallback(EventMessage message);
```

As we can see, the `EventMessage` object which we used for sending events is received in the callback. There will also be a higher level function available for handling event callbacks in a later version of the framework.

First we declare a method which satisfies the callback delegate
```csharp
public void CallbackReceived(EventMessage message)
{
    // Do Something with the received event
}

```

Then we use it with the `BasicConsume` function

```csharp
busProvider.BasicConsume("SomeRandomQueue", CallbackReceived);
```

The event which we sent in the first example uses a routing key which leads to the queue 'SomeRandomQueue'. This means that this event will be catched by this event listening function.

## Commands
Sending commands can be done by using the `CommandPublisher` class. The constructor takes a `IBusProvider` as parameter.

```csharp
ICommandPublisher commandPublisher = new CommandPublisher(busProvider);
```

**Note:** Right now, receiving commands is done via the `IBusProvider` class. This will be moved to the `CommandListener` class in the very near future.

### Sending commands
Sending commands can be done by using `Task<TResult> SendCommand<TResult>(object message, string queueName, string key, int timeout = 5000)`. The queue where the command is sent to should only be used for commands. Using the same queue for both commands and events is considered a very bad practice. The key should be unique for this type of command. **The key can't contain wildcards.**

The `SendCommand` method is async.

For this exapmle we would like to calculate the full name of a person. As parameter we use the following `Person` class.
```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

We can then send this `Person` instance as a parameter to a listening party.

```csharp
Person person = new Person
{
    FirstName = "Sjaak",
    LastName = "de Boer"
};

// We don't set the timeout, meaning it will have the default 5000ms as value
string fullName = await SendCommand<string>(person, "PersonCommandQueue", "PersonFullNameCommand");
```

### Listening to command messages
Right now we are refactoring this part a bit. This will be updated tomorrow.

# Attributes
Besides using the core functionality of the framework there also is an attribute implementation for listening to commands and events.

## Setting up the attributes
Using attributes requires to calls to dependency injection extension methods. This is very similar to using MVC.

```csharp
var options = new BusOptions();

var serviceProvider = new ServiceCollection()
    .AddRabbitMq(options)
    .BuildServiceProvider();

serviceProvider.UseRabbitMq();
```

## Listening to events
To listen to a event in a specific queue we use the `QueueListenerAttribute` and the `TopicAttribute` attributes.

```csharp
[QueueListener ("SomeRandomQueue")]
public class SomeController
{
    // the '*' and '#' wildcards can be used to listen to events
    [Topic ("foo.*.bar")]
    public void HandleFooBarEvent(PersonCreatedEvent personEvent)
    {
        // Do something with the event
    }
}
```

## Listening to commands
This will be updated in the very very very near future