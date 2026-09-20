using Microsoft.AspNetCore.Server.Kestrel.Https;
using MtlsBlazorClient;
using MtlsBlazorClient.Components;
using MtlsBlazorClient.Components.Pages;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// STEP 1 — Read API BaseUrl from appsettings.json
// ------------------------------------------------------------
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001/";


//Step 1 — Load certificate once at startup
var certPath = Path.Combine(builder.Environment.ContentRootPath, "clientcerts", "MtlsClient.pfx");
var cert = new X509Certificate2(
    certPath,
    "MizuhoMtlsPassword-12345",
    X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
);

//Step 2 — add the certificate to the HttpClient
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(cert);
handler.ServerCertificateCustomValidationCallback =
    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;


//Step 3. Register the HttpClient with the DI container, using the handler with the client certificate
builder.Services.AddHttpClient("MtlsClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => handler);


// the version use DI for AddSingleton(cert)
//// Register in DI
//builder.Services.AddSingleton(cert);

////STEP 2 — Use the DI certificate inside the handler
//builder.Services.AddHttpClient("MtlsClient", client =>
//{
//    client.BaseAddress = new Uri(apiBaseUrl);
//})
//.ConfigurePrimaryHttpMessageHandler(sp =>
//{
//    var cert = sp.GetRequiredService<X509Certificate2>();

//    var handler = new HttpClientHandler();
//    handler.ClientCertificates.Add(cert);

//    handler.ServerCertificateCustomValidationCallback =
//        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

//    return handler;
//});



// One step code (alternative approach) to register HttpClient with client certificate (mTLS)
// *****************************************************************************************
//builder.Services.AddHttpClient("MtlsClient", client =>
//{
//    client.BaseAddress = new Uri(apiBaseUrl);
//})
////.AddHttpMessageHandler<DebugHandler>()
//.ConfigurePrimaryHttpMessageHandler(() =>
//{
//    var certPath = Path.Combine(
//        builder.Environment.ContentRootPath, "clientcerts", "MtlsClient.pfx"
//    );

//    var cert = new X509Certificate2(
//        certPath,
//        "MizuhoMtlsPassword-12345",
//        X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
//    );

//    var handler = new HttpClientHandler();
//    handler.ClientCertificates.Add(cert);

//    // DEV ONLY
//    handler.ServerCertificateCustomValidationCallback =
//        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

//    return handler;
//});



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



// mTLS Workflow Diagram (End‑to‑End)
//┌──────────────────────────────┐
//│ Blazor Client(MtlsTest page) │
//└──────────────┬───────────────┘
//               │ 1.User clicks "CallApi()"
//               ▼
//┌──────────────────────────────────────────────┐
//│ HttpClient (with client certificate attached)│
//└──────────────┬───────────────────────────────┘
//               │ 2. Sends HTTPS request
//               │    + attaches client certificate
//               ▼
//┌──────────────────────────────────────────────┐
//│ TLS Handshake (Kestrel HTTPS endpoint)       │
//│ ClientCertificateMode = RequireCertificate   │
//└──────────────┬───────────────────────────────┘
//               │ 3. Server requests client cert
//               │ 4. Client sends certificate
//               │ 5. Kestrel validates certificate format
//               ▼
//┌──────────────────────────────────────────────┐
//│ ASP.NET Core Pipeline                        │
//│ HttpContext.Connection.ClientCertificate     │
//└──────────────┬───────────────────────────────┘
//               │ 6. Certificate is now available
//               ▼
//┌──────────────────────────────────────────────┐
//│ Authorization Handler (ClientCertificateHandler)
//│ [Authorize(Policy="ClientCertificate")]       │
//└──────────────┬───────────────────────────────┘
//               │ 7. Handler checks certificate:
//               │    - Subject
//               │    - Thumbprint
//               │    - Issuer
//               │    - EKU
//               ▼
//┌──────────────────────────────────────────────┐
//│ SecureController /api/secure/ping            │
//└──────────────┬───────────────────────────────┘
//               │ 8. Controller executes
//               ▼
//┌──────────────────────────────────────────────┐
//│ API Response ("pong")                        │
//└──────────────┬───────────────────────────────┘
//               │ 9. HttpClient receives response
//               ▼
//┌──────────────────────────────┐
//│ Blazor UI updates <pre>      │
//└──────────────────────────────┘
