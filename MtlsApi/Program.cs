using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using MtlsApi.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1. Load Kestrel HTTPS + mTLS settings from appsettings.json
// ------------------------------------------------------------
//builder.WebHost.ConfigureKestrel(options =>
//{
//    // This automatically picks up:
//    // - Url: https://localhost:5001
//    // - server.pfx certificate
//    // - ClientCertificateMode: Require
//    //
//    // Because you already defined them in appsettings.json:
//    //
//    // "Kestrel": {
//    //   "Endpoints": {
//    //     "Https": {
//    //       "Url": "https://localhost:5001",
//    //       "Certificate": { ... },
//    //       "ClientCertificateMode": "Require"
//    //     }
//    //   }
//    // }
//    //
//    options.Configure(builder.Configuration.GetSection("Kestrel"));
//});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
});


// ------------------------------------------------------------
// 2. Add Certificate Authentication (mTLS)
// ------------------------------------------------------------
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

                // Example validation: check CN
                if (cert.Subject.Contains("CN=MtlsClient"))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, "mtls-client")
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


//server is actually requiring client certificates
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ConfigureHttpsDefaults(httpsOptions =>
//    {
//        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
//        httpsOptions.CheckCertificateRevocation = false;
//    });
//});


// ------------------------------------------------------------
// 3. Add Authorization + Controllers
// ------------------------------------------------------------
//builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClientCertificate", policy =>
    {
        policy.Requirements.Add(new ClientCertificateRequirement());
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, ClientCertificateHandler>();

builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer(); for minimal APIs, not needed for controllers
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
