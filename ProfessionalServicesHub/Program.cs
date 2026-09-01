using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Clients;
using ProfessionalServicesHub.Components;
using ProfessionalServicesHub.Infrastructure.Data;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);

var syncfusionLicenseKey =
    builder.Configuration["Syncfusion:LicenseKey"];

if (string.IsNullOrWhiteSpace(syncfusionLicenseKey))
{
    throw new InvalidOperationException(
        "The Syncfusion license key is not configured. " +
        "See SETUP.md for configuration instructions.");
}

SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);

var connectionString = builder.Configuration
    .GetConnectionString("ProfessionalServicesHub")
    ?? throw new InvalidOperationException(
        "The ProfessionalServicesHub connection string is not configured.");

builder.Services.AddSyncfusionBlazor();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ClientQueryService>();
builder.Services.AddScoped<ClientCommandService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var factory = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

    await DevelopmentDataSeeder.SeedAsync(factory);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
