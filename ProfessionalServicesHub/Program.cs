using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Calendar;
using ProfessionalServicesHub.Application.Clients;
using ProfessionalServicesHub.Application.Documents;
using ProfessionalServicesHub.Application.Dashboard;
using ProfessionalServicesHub.Application.Work;
using ProfessionalServicesHub.Components;
using ProfessionalServicesHub.Infrastructure.Data;
using ProfessionalServicesHub.Infrastructure.Documents;
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

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 100 * 1024 * 1024;
});

builder.Services.AddMemoryCache();
builder.Services.AddSyncfusionBlazor();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ClientQueryService>();
builder.Services.AddScoped<ClientCommandService>();
builder.Services.AddScoped<ActivityBoardService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var factory = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

    await DevelopmentDataSeeder.SeedAsync(factory);
    await DevelopmentDataSeeder.SeedWorkAsync(factory);
    await DevelopmentDataSeeder.SeedCalendarAsync(factory);
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

app.MapGet(
    "/documents/{documentId:int}/download",
    DownloadDocumentAsync);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static async Task<IResult> DownloadDocumentAsync(
    int documentId,
    DocumentService documentService,
    CancellationToken cancellationToken)
{
    try
    {
        var download = await documentService.GetDownloadAsync(
            documentId,
            cancellationToken);

        return Results.Stream(
            download.Content,
            download.ContentType,
            download.FileName,
            enableRangeProcessing: true);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
}
