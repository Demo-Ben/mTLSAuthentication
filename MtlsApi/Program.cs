using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authorization;
using MtlsApi.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. Kestrel & mTLS Server Configuration
// ============================================================
// Configures Kestrel using endpoint definitions from appsettings.json,
// which enforces ClientCertificateMode.RequireCertificate for mTLS.
// This automatically picks up:
//      - Url: https://localhost:5001
//      - MtlsServer.pfx certificate
//      - ClientCertificateMode: Require
builder.WebHost.ConfigureKestrel(options =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
});

builder.Services.AddControllers();

// Alternative: Configure Kestrel programmatically instead of appsettings.json
// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     serverOptions.ListenAnyIP(5001, listenOptions =>
//     {
//         listenOptions.UseHttps("MtlsServer.pfx", "MtlsPassword-12345!", adapterOptions =>
//         {
//             adapterOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
//         });
//     });
// });



// ============================================================
// 2. Certificate Authentication Setup (mTLS Validation)
// ============================================================
// Registers certificate authentication and implements validation rules
// to map incoming client certificates into an authenticated ClaimsPrincipal.
builder.Services
    .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {
        options.AllowedCertificateTypes = CertificateTypes.All;

        options.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = context =>
            {
                var cert = context.ClientCertificate;

                // Validate the client certificate subject (case-insensitive match for MtlsClient)
                //if (cert.Subject.Contains("CN=MtlsClient"))
                if (cert.Subject.Contains("MtlsClient", StringComparison.OrdinalIgnoreCase))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, "mtls-client")
                        // Optional: Add roles or track the certificate thumbprint if needed
                        // new Claim(ClaimTypes.Role, "ApiClient"),
                        // new Claim(ClaimTypes.Thumbprint, cert.Thumbprint)
                    };

                    var identity = new ClaimsIdentity(claims, context.Scheme.Name);
                    context.Principal = new ClaimsPrincipal(identity);
                    context.Success();
                }
                else
                {
                    context.Fail("Invalid client certificate");
                }

                return Task.CompletedTask;
            }
        };
    });

// Instead of appsettings.json way. Uncomment the following code if you want to configure Kestrel programmatically
// instead of using appsettings.json.
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(5001, listenOptions =>
//    {
//        // Pass your pfx file, password, and an action to configure HttpsConnectionAdapterOptions
//        listenOptions.UseHttps("MtlsServer.pfx", "MtlsPassword-12345!", adapterOptions =>
//        {
//            adapterOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
//        });
//    });
//});


// ============================================================
// 3. Authorization Policy Registration
// ============================================================
// Defines a custom authorization policy requiring client certificate validation.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClientCertificate", policy =>
    {
        policy.Requirements.Add(new ClientCertificateRequirement());
    });
});


//This tells ASP.NET Core to use your custom certificate validator.
// Register the certificate validation handler
builder.Services.AddSingleton<IAuthorizationHandler, ClientCertificateHandler>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ------------------------------------------------------------
// 4. Middleware pipeline
// ------------------------------------------------------------
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
