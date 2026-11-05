using Dashboard.Api.Features.Customers;
using Dashboard.Api.Features.Employees;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Features.Products;

namespace Dashboard.Api.Realtime;

/// <summary>Events the server pushes to clients.</summary>
public interface IDashboardClient
{
    /// <summary>Sent to <see cref="Topics.Customers"/>.</summary>
    Task CustomerChanged(ChangeEvent<CustomerDto> change);

    /// <summary>Sent to <see cref="Topics.Orders"/> and the order's <see cref="Topics.CustomerOrders"/>.</summary>
    Task OrderChanged(ChangeEvent<OrderDto> change);

    /// <summary>Sent to <see cref="Topics.Employees"/>.</summary>
    Task EmployeeChanged(ChangeEvent<EmployeeDto> change);

    /// <summary>Sent to <see cref="Topics.Products"/>.</summary>
    Task ProductChanged(ChangeEvent<ProductDto> change);
}
