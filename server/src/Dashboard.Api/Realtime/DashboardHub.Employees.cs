using Dashboard.Api.Features.Employees;

namespace Dashboard.Api.Realtime;

public sealed partial class DashboardHub
{
    private const string Employee = "employee";

    public Task<HubResult<IReadOnlyList<EmployeeDto>>> SubscribeEmployees() =>
        SubscribeAsync(Topics.Employees, _employees.GetAllAsync);

    public Task UnsubscribeEmployees() => UnsubscribeAsync(Topics.Employees);

    public async Task<HubResult<EmployeeDto>> GetEmployee(int employeeId) =>
        Found(await _employees.GetAsync(employeeId, Aborted), Employee);

    public async Task<HubResult<EmployeeDto>> CreateEmployee(CreateEmployeeRequest request) =>
        Invalid<EmployeeDto>(request.Validate())
            ?? HubResult<EmployeeDto>.Success(await _employees.CreateAsync(request, Aborted));

    public async Task<HubResult<EmployeeDto>> UpdateEmployee(int employeeId, UpdateEmployeeRequest request) =>
        Invalid<EmployeeDto>(request.Validate())
            ?? HubResult<EmployeeDto>.From(await _employees.UpdateAsync(employeeId, request, Aborted), Employee);

    /// <summary>409 "In use" while any order names this employee as its sales person.</summary>
    public async Task<HubResult> DeleteEmployee(int employeeId, long? version) =>
        HubResult.From(await _employees.DeleteAsync(employeeId, version, Aborted), Employee);
}
