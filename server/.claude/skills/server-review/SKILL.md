---
name: server-review
description: Review uncommitted or branch changes in the .NET server (server/) against this project's rules in server/CLAUDE.md - hub/REST parity, HubResult envelopes, publishing after the lock, versions and concurrency, foreign-key-like checks, ID naming. Use when asked to review server code, a server diff, or before committing server changes.
---

# Server review

Review changes to the ASP.NET Core server in `server/` against the rules in `server/CLAUDE.md`. Read that file first; it is the source of truth, and this checklist only points at what most often goes wrong.

## 1. Find the changes
- Default target: uncommitted changes under `server/` (`git diff HEAD -- server/` plus untracked files from `git status --porcelain -- server/`)
- If the user names a branch, commit or PR, diff that against `main` instead
- Skip `bin/` and `obj/`
- Read each changed file in full, not just the hunks; most rules below are about how pieces fit together

## 2. Check against the project rules

**Hub and REST parity**
- Every hub operation has a REST endpoint calling the same service method with the same DTOs and validation, and the reverse
- Hub methods and endpoints only bind, validate, call the service and map the result; business logic or store access in them is a finding
- Subscribe methods join the group *before* reading the list
- Topic strings come only from `Realtime/Topics.cs`; never built inline or taken from the client

**Results and errors**
- Expected failures never throw: REST returns ProblemDetails, the hub returns `HubResult<T>` / `HubResult` with the same status codes
- 400 validation (field keys in camelCase `…ID` form), 404 missing, 409 concurrency conflict or "In use"
- Unknown IDs on `Get…` return 404, not `Ok` with null data

**Data store and publishing**
- Every read and write of `InMemoryDataStore` holds `store.Sync` for the whole operation
- Publishing through `IChangePublisher` happens after the lock is released and after the change is applied; never inside the lock, never before the write
- New or changed rows get a version from `NextVersion()`; IDs from the store's `Next…Id()`
- Order rows are built only through `OrderView`; `totalUsd` is computed, never stored; an order's version is the max of the order, customer, sales person and product versions
- A change to a customer, employee or product publishes `updated` events for the orders that show it; a customer delete publishes `deleted` for their orders
- Order changes publish to both `orders` and `customer:{id}:orders`

**References**
- Order writes reject an unknown `salesPersonID` or `productID` with 400 on that field
- Deleting an employee or product still used by orders returns 409 "In use"; deleting a customer deletes their orders

**Contracts and naming**
- DTOs are records; entities never leave services (not in results, events or endpoints)
- ID properties end in `ID` (`CustomerID`, `SalesPersonID`)
- Event payloads use the same DTO as the matching hub result and REST response, and include `Version`
- A changed DTO shape or hub method signature is a breaking change for `ui/`: call it out, but don't edit `ui/`

**General**
- Async all the way; every service, endpoint and hub method takes and passes a `CancellationToken` (hub: `Context.ConnectionAborted`)
- `ILogger<T>` with message templates, no string interpolation in log calls
- `TimeProvider` instead of `DateTime.UtcNow`
- No new NuGet packages unless the user approved them
- No secrets in code or config
- `server/CLAUDE.md` updated when the change alters a documented rule, topic, method or shape

Also look for plain correctness bugs: wrong status codes, missing null checks, race conditions between check and write, off-by-one in validation ranges.

## 3. Verify
Run from `server/`:
- `dotnet build`, building into a scratch folder with `-o` if the user's server is running and locks `bin/`
- `dotnet format --verify-no-changes`
- `dotnet test` once `tests/Dashboard.Api.Tests/` exists

Don't stop or kill the user's running server to do this.

## 4. Report
- List findings most severe first. For each: `file:line`, the rule or bug, and a concrete failing scenario (input → wrong result)
- Mark each as **bug**, **rule** (breaks a CLAUDE.md convention) or **nit**
- Say what you verified (build, format, tests) and what you didn't
- If nothing is wrong, say so plainly
- Don't edit files unless the user asks for fixes
