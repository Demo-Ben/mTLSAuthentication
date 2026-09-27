using MtlsBlazorClient.Components;
using MtlsBlazorClient.Services;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

var apiRemoteApiUrl = builder.Configuration["ApiSettings:RemoteApiUrl"] ?? "https://localhost:5000/";

// 2. Load the certificate from disk using its password and storage flags
// - PersistKeySet: Keeps the key container alive so Windows can access the private key securely
// - Exportable: Allows the key to be exported if needed later
var certPath = Path.Combine(builder.Environment.ContentRootPath, "clientcerts", "MtlsClient.pfx");
var cert = new X509Certificate2(
    certPath,
    "MizuhoMtlsPassword-12345",
    X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
);


// **********************************************************************************
// First approach (commented out) to register HttpClient with client certificate (mTLS)
// **********************************************************************************
// 3. Configure the HTTP message handler to attach the client certificate for mTLS
//var handler = new HttpClientHandler();
//handler.ClientCertificates.Add(cert);

// 4. Bypass strict server certificate validation (helpful for local development with self-signed server certs)
// WARNING: Remove or replace with proper trust logic in production environments.
//handler.ServerCertificateCustomValidationCallback =
//    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

// 5. Register a named HttpClient in Dependency Injection using this custom handler
//builder.Services.AddHttpClient("MtlsClient", client =>
//{
//    client.BaseAddress = new Uri(apiRemoteApiUrl);
//})
//.ConfigurePrimaryHttpMessageHandler(() => handler);
// **********************************************************************************


// **********************************************************************************
// Second approach (preferred) to register HttpClient with client certificate (mTLS)
// **********************************************************************************

// 3. Register the certificate instance as a Singleton in DI so it can be shared cleanly across services
builder.Services.AddSingleton(cert);


// ------------------------------------------------------------
// STEP 3 — Register a service to fetch JWT tokens
// ------------------------------------------------------------
// STEP 4 — Register HttpClient that sends BOTH mTLS + JWT
// ------------------------------------------------------------
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();


// 4. Register the named HttpClient and inject the certificate from the DI container into the message handler
builder.Services.AddHttpClient("ApiClient", async (sp, client) =>
{
    client.BaseAddress = new Uri(apiRemoteApiUrl);

    // ** Register a service to fetch JWT tokens
    //var jwtService = sp.GetRequiredService<IJwtTokenService>();
    //var token = await jwtService.GetTokenAsync();
    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    // ** Register HttpClient that sends mTLS
    var cert = sp.GetRequiredService<X509Certificate2>();

    var handler = new HttpClientHandler();
    handler.ClientCertificates.Add(cert);

    //Bypass strict server validation for local self-signed dev certificate   
    handler.ServerCertificateCustomValidationCallback =
           HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

    return handler;
});
// **********************************************************************************



// One step code (alternative approach) to register HttpClient with client certificate (mTLS)
// *****************************************************************************************
//builder.Services.AddHttpClient("MtlsClient", client =>
//{
//    client.BaseAddress = new Uri(apiRemoteApiUrl);
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


//The API only trusts certificates issued by YOUR CA.

//Your Company(CA)
//    ↓ issues
//Client Certificate (for Third Party)
//    ↓ installed in
//Third Party System
//    ↓ used by
//Third Party HttpClient
//    ↓ during TLS handshake
//Your API (validates certificate)


//The 4 things your API must check to confirm the certificate is valid
//Your API must validate:
//    Certificate chain
//    Certificate thumbprint
//    Certificate subject
//    Enhanced Key Usage (EKU)