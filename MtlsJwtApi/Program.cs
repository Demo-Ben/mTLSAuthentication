using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.IdentityModel.Tokens;
using MtlsJwtApi.Authorization;
using MtlsJwtApi.Authorization;
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
// 2. Authentication: mTLS + JWT Bearer
// ============================================================
// Registers certificate authentication and implements validation rules
// to map incoming client certificates into an authenticated ClaimsPrincipal.
builder.Services
    //.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddAuthentication(options =>
    {
        // IMPORTANT: allow BOTH schemes
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    // ------------------ mTLS ------------------
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
    })
    // ------------------ JWT ------------------
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];   // e.g. https://login.microsoftonline.com/{tenant}
        options.Audience = builder.Configuration["Jwt:Audience"];    // your API App ID

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
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
// 3. Authorization: require BOTH mTLS + JWT
// ============================================================
// Defines a custom authorization policy requiring client certificate validation.
builder.Services.AddAuthorization(options =>
{
    // mTLS policy
    options.AddPolicy("ClientCertificate", policy =>
    {
        policy.Requirements.Add(new ClientCertificateRequirement());
    });

    // Combined policy: JWT + mTLS
    options.AddPolicy("JwtAndMtls", policy =>
    {
        policy.RequireAuthenticatedUser(); // JWT must be valid
        policy.Requirements.Add(new ClientCertificateRequirement()); // mTLS must be valid
    });
});


//This tells ASP.NET Core to use your custom certificate validator.
//4. Register your mTLS handler
builder.Services.AddSingleton<IAuthorizationHandler, ClientCertificateHandler>();

builder.Services.AddControllers();
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
