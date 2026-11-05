# Project
Backend for the customer dashboard. ASP.NET Core (.NET 10), C# with nullable enabled.
Persistence: none; all data is held in memory and resets when the server restarts (see Data).
APIs:
- SignalR hub at `/hub`: the client API. The React client does all CRUD over it (lists, single rows, creates, updates, deletes) and receives live updates
- REST (Minimal APIs) at `/api`: the same operations, for debugging and verification (browser, curl). The React client never calls it
- Swagger page at `/swagger` for trying the REST endpoints, built from the OpenAPI description at `/openapi/v1.json` (Microsoft.AspNetCore.OpenApi + Swashbuckle.AspNetCore.SwaggerUI). Development only; the hub isn't in it
Runs on http://localhost:5001; the ui dev server (http://localhost:5000) proxies `/hub` here.

# Commands
Run from `server/`:
- `dotnet build`
- `dotnet run --project src/Dashboard.Api`
- `dotnet test`
- `dotnet format`: run before calling a task done, along with build and test

# Structure
```
server/
├── Dashboard.sln
├── src/Dashboard.Api/
│   ├── Program.cs                  # DI, middleware, MapGroup("/api"), MapHub("/hub"), OpenAPI + Swagger (Development)
│   ├── Features/
│   │   ├── Customers/
│   │   │   ├── CustomerEndpoints.cs    # MapCustomerEndpoints(this RouteGroupBuilder); debug REST
│   │   │   ├── CustomerDtos.cs         # request/response records, shared by hub and REST
│   │   │   └── CustomerService.cs      # business logic + publishes changes
│   │   ├── Orders/                     # same shape as Customers
│   │   ├── Employees/                  # same shape as Customers
│   │   └── Products/                   # same shape as Customers
│   ├── Realtime/
│   │   ├── DashboardHub.cs         # Hub<IDashboardClient>: shared helpers; partial class
│   │   ├── DashboardHub.<Domain>.cs  # one partial per domain: subscribe (returns initial list), unsubscribe, get, create, update, delete
│   │   ├── IDashboardClient.cs     # server → client events
│   │   ├── Topics.cs               # group-name builders; the only place topic strings exist
│   │   ├── ChangeEvent.cs          # pushed-change envelope
│   │   ├── HubResult.cs            # result envelope for hub method calls
│   │   └── ChangePublisher.cs      # IChangePublisher; wraps IHubContext
│   ├── Data/
│   │   ├── InMemoryDataStore.cs    # all rows + versions, for the app's lifetime
│   │   ├── SeedData.cs             # starting data, loaded on every start
│   │   └── Entities/               # stored rows; never returned from endpoints or the hub
│   └── Common/                     # ProblemDetails, validation helpers, extensions
└── tests/Dashboard.Api.Tests/      # xUnit; mirrors src folder layout
```

## Where new code goes
- New domain → `Features/<Domain>/` with Endpoints, Dtos, Service; hub methods in `Realtime/DashboardHub.<Domain>.cs`; register in Program.cs
- New operation → service method first, then the hub method the client uses, then the matching REST endpoint for debugging
- New kind of data or field → the entity, then `InMemoryDataStore` and `SeedData`, then DTOs, then ui
- New real-time topic → add a builder in `Topics.cs`, subscribe/unsubscribe methods in the hub, an event in `IDashboardClient`

# Conventions
- Async all the way; every service method, endpoint and hub method takes a `CancellationToken` (hub: `Context.ConnectionAborted`)
- DTOs are `record`s; never expose entities from endpoints, hub results or hub events
- ID properties in DTOs and requests end in `ID` (`CustomerID`, `SalesPersonID`); JSON is camelCase, so they serialize as `customerID`, `salesPersonID`, and validation error keys use the same names
- Services hold the logic; endpoints and hub methods only bind, validate, call the service, and map results
- Hub and REST call the same service methods and use the same DTOs and validation, so REST always shows what the hub sees
- Never throw for expected failures:
  - REST returns ProblemDetails (`Results.Problem` / `Results.ValidationProblem`)
  - Hub methods return `HubResult<T>` with the same status codes and field errors
- 404 for missing entities, 409 for concurrency conflicts, 400 for validation
- Services enforce references between rows like foreign keys:
  - An order's `SalesPersonID` must be an existing employee and its `ProductID` an existing product, or the write fails with 400 and an error on that field
  - Deleting an employee or product that any order uses fails with 409 "In use"
  - Deleting a customer also deletes their orders
- REST list endpoints return a plain JSON array of every row, sorted by ID: the same list the matching hub subscribe returns. No paging
- Optimistic concurrency uses the row `Version`, exposed as `version` in DTOs
- Structured logging with `ILogger<T>` and message templates; no string interpolation in log calls
- Use `TimeProvider` instead of `DateTime.UtcNow`; keep timestamps in UTC
- Don't add NuGet packages without asking

# Data
- `InMemoryDataStore` (a singleton) holds every row for the app's lifetime. It starts from `SeedData` on each start, so changes are lost when the server restarts
- Reads and writes hold `store.Sync` for the whole operation; publish after releasing it, never while holding it
- The store assigns IDs (`NextCustomerId()` etc., continuing after the highest seeded ID) and versions (`NextVersion()`, one shared counter)
- Versions restart when the server does; clients reload their lists on reconnect, so they pick up new versions
- Order totals are computed, never stored

# Real-time (SignalR)
## Subscription model
- One SignalR connection per browser tab, shared by all views; never a connection per view
- Clients subscribe to topics that match what they are viewing; a topic is a SignalR group
- Topics (built only through `Topics.cs`):
  - `customers`: all customer changes (Customer List page)
  - `orders`: all order changes, every customer (Orders page)
  - `customer:{customerId}:orders`: order changes for one customer (Customer Orders page)
  - `employees`: all employee changes
  - `products`: all product changes
- A client may hold several subscriptions at once; don't assume one active view
- Subscribe methods return the topic's current list, so one call gives the client its data and starts its updates:
  - `SubscribeCustomers()` → `HubResult<IReadOnlyList<CustomerDto>>`
  - `SubscribeOrders()` → `HubResult<IReadOnlyList<OrderDto>>`
  - `SubscribeCustomerOrders(int customerId)` → `HubResult<IReadOnlyList<OrderDto>>`; 404 if the customer doesn't exist
  - `SubscribeEmployees()` → `HubResult<IReadOnlyList<EmployeeDto>>`
  - `SubscribeProducts()` → `HubResult<IReadOnlyList<ProductDto>>`
  - Each has a matching `Unsubscribe…` method that returns nothing
- A subscribe method joins the group first, then reads the list. Never the other way round: a change committed in between would be missed
- The list is the whole topic, sorted by ID, not paged; the matching REST GET returns the same list
- Never accept raw topic strings from the client
- Subscribe methods validate input and check the caller may see that data before adding to the group
- Group membership is lost when a connection is replaced; the client re-subscribes after reconnect and replaces its list with the one returned (server keeps no subscription state)

## CRUD over the hub
- Typed per operation, mirroring REST:

  | Domain | Read one | Create | Update | Delete |
  |---|---|---|---|---|
  | Customers | `GetCustomer(int customerId)` | `CreateCustomer(CreateCustomerRequest)` | `UpdateCustomer(int customerId, UpdateCustomerRequest)` | `DeleteCustomer(int customerId, long? version)` |
  | Orders | `GetOrder(int orderId)` | `CreateOrder(int customerId, CreateOrderRequest)` | `UpdateOrder(int orderId, UpdateOrderRequest)` | `DeleteOrder(int orderId, long? version)` |
  | Employees | `GetEmployee(int employeeId)` | `CreateEmployee(CreateEmployeeRequest)` | `UpdateEmployee(int employeeId, UpdateEmployeeRequest)` | `DeleteEmployee(int employeeId, long? version)` |
  | Products | `GetProduct(int productId)` | `CreateProduct(CreateProductRequest)` | `UpdateProduct(int productId, UpdateProductRequest)` | `DeleteProduct(int productId, long? version)` |

  Lists come from the `Subscribe…` methods above
- Gets and writes return `HubResult<T>` with the DTO (no data for deletes); a get returns 404 for an unknown ID
- Every hub method also has a REST endpoint calling the same service method
- The caller gets the change twice, as its result and as the pushed event; clients apply whichever arrives first and drop the other by `Version`
- Check the caller may make the change before calling the service

## Hub rules
- The hub is a thin adapter like the REST endpoints: validate, call the service, map to `HubResult<T>`
- Keep business logic and data access out of the hub; it goes in services
- `HubResult<T>(bool Ok, T? Data, HubError? Error)`; `HubError(int Status, string Title, string? Detail, IDictionary<string, string[]>? Errors)` mirrors ProblemDetails so the client handles both the same way
- Unexpected failures may throw; SignalR reports them to the caller without details unless detailed errors are on (Development only)

## Publishing changes
- Services publish through `IChangePublisher` only after the change is applied to the store; never before
- Changes are published the same way whether they came from the hub or from REST
- A change to an order publishes to `orders` and to `customer:{customerId}:orders`; a client on both
  can get it twice and drops the duplicate by `Version`
- A change to a customer publishes to `customers`; to an employee, `employees`; to a product, `products`
- Deleting a customer publishes a `deleted` event for each of their orders as well as for the customer
- `OrderDto` is a display row: `orderID`, `customer { customerID, firstName, middleInitial, lastName }`,
  `salesPerson { employeeID, firstName, middleInitial, lastName }`, `product { productID, name }`, `quantity`, `totalUsd`, `version`
  - `totalUsd` is the product's current price × quantity, computed when the row is built, not stored; a price change updates it
  - Built in one place, `Features/Orders/OrderView.cs`; writes still take `salesPersonID` and `productID`
  - Its `version` is the highest of the order's own and its customer's, sales person's and product's versions, so a change to anything the row shows makes it newer; update and delete check concurrency against this version
  - Updating a customer, employee or product (including a product's price) also publishes an `updated` order event for every order that shows it
- Envelope: `ChangeEvent<T>(string Type, int Id, long Version, T? Data)` with Type `created` | `updated` | `deleted`; Data is null for deleted
- Always include `Version` so clients can drop stale or duplicate events
- Event payloads use the same DTO as the hub results and REST endpoints for that resource

# Migration notes (remove each line once done)
- `tests/Dashboard.Api.Tests/` doesn't exist yet; it needs xUnit packages added
<!-- 
## Scaling (not needed yet)
- A single instance needs no backplane
- More than one instance would need shared storage instead of in-memory data, plus a backplane (such as Redis) so publishes reach all connections
 -->
