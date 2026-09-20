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
    var certPath = Path.Combine(
        builder.Environment.ContentRootPath, "clientcerts", "MtlsClient.pfx"
    );

    // Load client certificate from file (dev only)
    var clientCert = new X509Certificate2(
        certPath,                            // or hard code: "clientcerts/MtlsClient.pfx",
        "MizuhoMtlsPassword-12345",
        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable
    );

    var handler = new HttpClientHandler();
    handler.ClientCertificates.Add(clientCert);  //    // Attach client certificate for mTLS

    // DEV ONLY: accept any server certificate
    handler.ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});


//builder.Services.AddHttpClient("MtlsClient", client =>
//        {
//            client.BaseAddress = new Uri(apiBaseUrl);
//        })
//        .ConfigurePrimaryHttpMessageHandler(() =>
//        {
//            var certPath = Path.Combine(
//                builder.Environment.ContentRootPath, "clientcerts", "MtlsClient.pfx"
//            );

//            var clientCert = new X509Certificate2(
//                certPath,
//                "MizuhoMtlsPassword-12345",
//                X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
//            );

//            var handler = new HttpClientHandler();
//            handler.ClientCertificates.Add(clientCert);

//            // DEV ONLY
//            handler.ServerCertificateCustomValidationCallback =
//                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

//            return handler;
//        });



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
