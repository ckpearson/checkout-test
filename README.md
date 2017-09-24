# Checkout.com test repo

This repo contains the solutiuon for the checkout.com technical test, it contains the following projects:

1. api - this is the .net core webapi project
2. api-tests - this is an xunit test project that while not exhaustive, covers the fundamentals and major functionality of the api and its utils.
3. api-client - this is a class library that exposes (using [Refit](https://github.com/paulcbetts/refit)) an interface implementation that can communicate with the API.

# Notable assumptions

1. The API performs no authentication as this wasn't specified; though it could be added in various forms down the line.
2. All API operations are **GET** operations, there wasn't any major body data to be sent over the wire, so I didn't deem having post methods as necessary.
3. The API has the notion of "active" and "completed" orders; it supports "completing" an order as a simulation of completion.
4. As the spec stated storage could be simulated, the API and tests both do this with a backing dictionary.

# Basic structure of the API

The API follows a fairly conventional storage -> access -> presentation architecture:

1. A core IDataStore implementation provides the general CRUD operations
2. Individual model services access the store (and other services) to provide scoped functionality against the store
3. The controllers then make use of the services for performing their work

# Extent of the tests

1. The tests cover the core utility classes and functionality to ensure that they're solid enough to rely on
2. The services are covered, and the store's functionality is ensured through these

# Use of Option and Result monads

1. In order to better represent and reason about "no-value" cases the Option monad is employed over null values
2. In order to deal with computations that can fail for a number of reasons, the Result monad is used over exceptions

Various binding methods are used to allow for the basic short-circuiting on the fail-path.

Extension methods for adding support for LINQ query expression syntax for both of the monads are also present, for allowing easier more declarative use of them.

# Use of Refit for the client library

I initially considered utilising [Swagger](https://swagger.io/) by means of the [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) package, but I decided against this as:

1. A number of controller actions return `IActionResult` so Swashbuckle wouldn't have been able to properly infer the return type
2. The spec stated that a client library was required, and I felt "derive one yourself using Swagger" didn't fully meet that criterion.

Instead, by using Refit, all the hard work of using `HttpClient` is dealt with by means of an auto-generated proxy implementation.

# Things I'd change if this was taken forward

1. I'd move to using a proper data store
2. The unit tests would be rewritten to not rely on the references being stable for the store-returned objects, and would instead assert against the returned instances for things like the order operations
3. I'd factor the model classes out into a class library so the projects could share them, rather than the client library redefining them
    * Although I decided this was fine given the limited number and scope of the problem at this stage.
4. I would modify the Result monad returning operations to provide a more structured error, such that the controllers could infer the correct status code to return, rather than automatically assuming HTTP-500 for most cases.
