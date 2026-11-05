using Dashboard.Api.Common;
using Dashboard.Api.Data;
using Dashboard.Api.Features.Customers;
using Dashboard.Api.Features.Employees;
using Dashboard.Api.Features.Orders;
using Dashboard.Api.Features.Products;
using Dashboard.Api.Realtime;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              // SignalR's browser client sends credentials when it negotiates
              .AllowCredentials());
});

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddSignalR(options => options.EnableDetailedErrors = builder.Environment.IsDevelopment());

// In memory only; resets on restart
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<IChangePublisher, ChangePublisher>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<ProductService>();

// Behind Caddy: take the client IP, scheme and host from X-Forwarded-*. Loopback is
// trusted by default; add any other proxy's network to ForwardedHeaders:KnownNetworks
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    foreach (var network in builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [])
        options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
});

var app = builder.Build();

// First, so everything after it sees the client's values
app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();

// Swagger for the REST endpoints (not the hub), Development only
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Dashboard API v1"));
}

var api = app.MapGroup("/api").DisableHttpCaching();
api.MapGet("/hello", () => Results.Ok(new { message = "Hello, World!" }));
api.MapCustomerEndpoints();
api.MapOrderEndpoints();
api.MapEmployeeEndpoints();
api.MapProductEndpoints();

app.MapHub<DashboardHub>(DashboardHub.Path);
app.MapHealthChecks("/health");

app.Run();

// Lets the test project start the app with WebApplicationFactory<Program>
public partial class Program;
