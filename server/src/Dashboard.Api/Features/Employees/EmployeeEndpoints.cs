using Dashboard.Api.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dashboard.Api.Features.Employees;

public static class EmployeeEndpoints
{
    private const string EntityName = "employee";

    public static RouteGroupBuilder MapEmployeeEndpoints(this RouteGroupBuilder api)
    {
        var employees = api.MapGroup("/employees").WithTags("Employees");

        // The same list SubscribeEmployees returns
        employees.MapGet("/", async Task<Ok<IReadOnlyList<EmployeeDto>>> (EmployeeService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(ct)));

        employees.MapGet("/{employeeId:int}", async Task<Results<Ok<EmployeeDto>, NotFound>> (
            int employeeId, EmployeeService service, CancellationToken ct) =>
            await service.GetAsync(employeeId, ct) is { } employee
                ? TypedResults.Ok(employee)
                : TypedResults.NotFound());

        employees.MapPost("/", async Task<Results<Created<EmployeeDto>, ValidationProblem>> (
            CreateEmployeeRequest request, EmployeeService service, CancellationToken ct) =>
        {
            var errors = request.Validate();
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            var employee = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/employees/{employee.EmployeeID}", employee);
        });

        employees.MapPut("/{employeeId:int}",
            async Task<Results<Ok<EmployeeDto>, NotFound, ProblemHttpResult, ValidationProblem>> (
                int employeeId, UpdateEmployeeRequest request, EmployeeService service, CancellationToken ct) =>
        {
            var errors = request.Validate();
            if (errors.Count > 0) return TypedResults.ValidationProblem(errors);

            var result = await service.UpdateAsync(employeeId, request, ct);
            return result.Status switch
            {
                WriteStatus.Ok => TypedResults.Ok(result.Value!),
                WriteStatus.NotFound => TypedResults.NotFound(),
                _ => ProblemResults.Conflict(EntityName),
            };
        });

        // ?version= is optional; when sent, the delete only happens if it still matches
        employees.MapDelete("/{employeeId:int}", async Task<Results<NoContent, NotFound, ProblemHttpResult>> (
            int employeeId, long? version, EmployeeService service, CancellationToken ct) =>
            await service.DeleteAsync(employeeId, version, ct) switch
            {
                WriteStatus.Ok => TypedResults.NoContent(),
                WriteStatus.NotFound => TypedResults.NotFound(),
                WriteStatus.InUse => ProblemResults.InUse(EntityName),
                _ => ProblemResults.Conflict(EntityName),
            });

        return api;
    }
}
