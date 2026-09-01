using ProfessionalServicesHub.Components;
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

builder.Services.AddSyncfusionBlazor();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

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