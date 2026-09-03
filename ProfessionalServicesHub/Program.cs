using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Calendar;
using ProfessionalServicesHub.Application.Clients;
using ProfessionalServicesHub.Application.Documents;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Application.Dashboard;
using ProfessionalServicesHub.Application.Work;
using ProfessionalServicesHub.Components;
using ProfessionalServicesHub.Components.Account;
using ProfessionalServicesHub.Components.Security;
using ProfessionalServicesHub.Components.Services;
using ProfessionalServicesHub.Infrastructure.Data;
using ProfessionalServicesHub.Infrastructure.Documents;
using ProfessionalServicesHub.Infrastructure.Identity;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;
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

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider,
    IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(
        AppPolicies.ManageConfiguration,
        policy => policy.RequireRole(AppRoles.Administrator))
    .AddPolicy(
        AppPolicies.DispatchWork,
        policy => policy.RequireRole(
            AppRoles.Administrator,
            AppRoles.Coordinator))
    .AddPolicy(
        AppPolicies.ManageClients,
        policy => policy.RequireRole(
            AppRoles.Administrator,
            AppRoles.Coordinator));

builder.Services.AddScoped<ICurrentUserAccessor,
    BlazorCurrentUserAccessor>();
builder.Services.AddScoped<EngagementAccessService>();

builder.Services.AddScoped<ClientQueryService>();
builder.Services.AddScoped<ClientCommandService>();
builder.Services.AddScoped<ClientLookupService>();
builder.Services.AddScoped<EngagementQueryService>();
builder.Services.AddScoped<ActivityBoardService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<UiNotificationService>();
builder.Services.AddScoped<SfDialogService>();

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

await IdentitySeeder.EnsureIdentityAsync(
    app.Services,
    app.Configuration,
    app.Environment);

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

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapGet(
    "/documents/{documentId:int}/download",
    DownloadDocumentAsync)
    .RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static async Task<IResult> DownloadDocumentAsync(
    int documentId,
    ClaimsPrincipal principal,
    DocumentService documentService,
    CancellationToken cancellationToken)
{
    try
    {
        var download = await documentService.GetDownloadAsync(
            documentId,
            principal,
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
