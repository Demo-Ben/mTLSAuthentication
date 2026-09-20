using MtlsBlazorClient.Components;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// STEP 1 — Read API BaseUrl from appsettings.json
// ------------------------------------------------------------
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001/";

// ------------------------------------------------------------
// STEP 2 — Register HttpClient with client certificate (mTLS)
// ------------------------------------------------------------
//
// This HttpClient will be used to call the protected API.
// It attaches the client certificate required by the server.
//
builder.Services.AddScoped(sp =>
{
    // Load client certificate from file (dev only)
    var clientCert = new X509Certificate2(
        "clientcerts/MtlsClient.pfx",
        "MizuhoMtlsPassword-12345!",
        X509KeyStorageFlags.MachineKeySet |
        X509KeyStorageFlags.Exportable
    );

    var handler = new HttpClientHandler();

    // Attach client certificate for mTLS
    handler.ClientCertificates.Add(clientCert);

    // DEV ONLY: accept any server certificate
    handler.ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});

// ------------------------------------------------------------
// STEP 3 — Add Blazor components
// ------------------------------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ------------------------------------------------------------
// STEP 4 — Build the app
// ------------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------------
// STEP 5 — Middleware pipeline
// ------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ------------------------------------------------------------
// STEP 6 — Map Blazor root component
// ------------------------------------------------------------
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
