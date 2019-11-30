# OCore

Opinionated and experimental application stack built on Microsoft Orleans and friends.

Features (partially to come, look at this as a TODO list in no particular order):

- Service publishing (cluster boundaries defined over HTTP) _Partially working_
    - Service Client
- Event aggregation (based on Orleans streams)
- Authentication
    - User accounts with optional tenancy
    - API keys
        - Resource bound
        - Rate limiting
- Authorization
- Multi tenancy
- Rich entities (Grain subclassing to add more information in backing store)
    - Collection querying (for select backends)
- Data entities
    - HTTP exposure
    - Auto CRUD
- Audited entities
- Data polling
- Idempotent actions

# Motivation

Programming is fun. Plumbing is not.

# Setup

## Development

Look at the sample for an example to setup OCore quickly using default configuration which means:

- Automatically register and publish all Services with HTTP endpoints
- Automatically register Data Entities with HTTP endpoints

Get started (silly developer wrapper on the hostbuilder setup):

```csharp

        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();
            await hostBuilder.LetsGo(typeof(Services.Zoo));
            
            Console.ReadLine();
        }
```

## Service setup

Documentation TODO, functionality currently lives in `OCore.Authorization`.

# Service 

An OCore Service is a stateless, reentrant integer keyed grain. It can _optionally_ be automatically published to an HTTP endpoint. An exposed Service is an opinionated alternative to an ASPNET Core `Controller`, with these benefits:

- No plumbing to get to the cluster
- No chance of accidentally putting business logic in the controllers
- Easy to maintain
- Easy to learn
- Plays (relatively) nice with the rest of ASPNET Core

Define an interface to the service. Decide mentally if you want this to be an external or internally available interface. 

```csharp
[Service("MyService")]
public interface IMyService : IService {
    Task<string> Hello(string name);
}
```

Then implement it:

```csharp
public class MyService : Service {
    public Task<string> Hello(string name) {
        return Task.FromResult($"Hello, {name}");
    }
}
```

Using Postman or the Visual Studio Code REST client (I will be using this in the coming examples, and you can find a bunch of `.http`-files in this repository to help testing):

```
### Say Hello
POST http://localhost:9000/service/MyService/Hello

["COCPORN"]
```

The service will then respond with a 200-message with a string HTTP body of `"Hello, COCPORN"`.

## Parameter passing

Parameters can be passed in these ways:

- A list of parameters
- A single request object
- Empty body if there are no parameters, or if all parameters are default parameters

Passing a single request object is short hand for passing a list with a single entry request object.

Parameter lists support complex types and default parameters.

## Internal services

If you do not want to publish any services, do not call `MapServices` (this is called automatically from `UseDefaultOCore()`). If you want to stop specific services from being published, you can decorate them with the `[Internal]`-attribute. If you want specific methods on a service to not be published, you can similarly decorate them with the `[Internal]`-interface.

```csharp
[Service("PrivateService")]
[Internal]
public interface IPrivateService : IService 
{
    Task<Guid> TellMeASecret(int left, int right);
}

[Service("ShyService")]
public interface IShyService: IService 
{
    Task<Joke> TellMeAJoke();

    [Internal]
    Task<string> GiveMeAWinningLotteryNumber();
}
```

## HTTP calls

You can decorate methods with attributes that implement `IAuthorizationFilter`:

```csharp
[Service("AuthorizedService")]
public interface IAuthorizedService : IService 
{
    [Authorize]
    Task<Guid> TellMeASecret(int left, int right);
}
```

There will be support for `ActionFilter`s.

## Service client

There is currently a loosely typed Service client that works in Blazor and other dotnet projects (the interface for this _will change_):

```csharp
var (response, status) = await Client.Invoke<IMyService, string>("Hello", "COCPORN");
```

I am toying with the idea of making a strongly typed client using Roslyn code generation.

# Data Entities

An OCore Data Entity is promiscuous, providing full access to its internal state. Data Entities can _optionally_ serve their innards over HTTP. They can also be extended with commands.

## Implicit access

A Data Entity implicitly provides these methods:

- `Create` (mapped to `POST`)
- `Read` (mapped to `GET`)
- `Update` (not mapped)
- `Upsert` (mapped to `PUT`)
- `Delete` (mapped to `DELETE`)

If an entity is not created, all calls except `Create`/`POST` and `Upsert`/`PUT` will fail.

## Example

```csharp
public class ShortenedUrlState 
{
    public string RedirectTo { get; set; }

    public int TimesVisited { get; set; }
}

[DataEntity("ShortenedUrl")]
public interface IShortenedUrl : IDataEntity<ShortenedUrlState> 
{
    Task<string> Visit();
}
```

...with the implementation:


```csharp
public class ShortenedUrlDataEntity : DataEntity<ShortenedUrlState>, IShortenedUrl
{
    public async Task<string> Visit() 
    {
        State.TimesVisited++;
        await WriteStateAsync();
        return State.RedirectTo;
    }
}
```

If `MapDataEntities` is called (as is default by `UseDefaultOCore`), you can now do:

```
### Create shortened URL
POST http://localhost:9000/data/ShortenedUrl/SomeId

{
    "RedirectTo": "http://www.cocporn.com"
}
```

This will create the entity.

```
### Get shortened URL data object
GET http://localhost:9000/data/ShortenedUrl/SomeId
```
...will return:
```
{
    "RedirectTo": "http://www.cocporn.com",
    "TimesVisited": 0
}
```

Call `Visit`-method:

```
### "Visit" the Data Entity
POST http://localhost:9000/data/ShortenedUrl/SomeId/Visit
```

This will call the `Visit`-method updating the counter, and return the `RedirectTo`-string.

## Multifetch

Using `GET`, you can do multifetch using HTTP:

```
### Multifetch
GET http://localhost:9000/data/SomeDataEntity/Id1,Id2,Id5
```

This will return:

```
[
    {
        "data": "id1-data"
    },
    null,
    {
        "data": "id5-data"
    }
]
```

This indicates that the system was able to fetch data for Id1 and Id5, but not for Id2.

## Authorization

TODO: Data Entities will play nice with the authorization and tenancy system, so they are not _that_ promiscuous. :)

# `Entity<T>`

`Entity<T>` is what Data Entities and others are based on. They provide a helpful layer in addition to `Grain<T>` by:

- The Grain key is deconstructed into `KeyString`, `KeyGuid`, `KeyLong` and `KeyExtension`
- `Entity<T>` adds tenant information so that it is easy to get to, using `TenantId`
- API is very similar to that of `Grain<T>`, so in most cases it will be a drop-in replace. **NOTE**: The shape of the stored data is _different_, so you cannot change this after you have started storing data. If in doubt, just use `Entity<T>` for everything