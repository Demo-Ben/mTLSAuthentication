using Microsoft.AspNetCore.Authentication.Certificate;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Configure(builder.Configuration.GetSection("Kestrel"));
});



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

// Server is actually requiring client certificates
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        //httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        httpsOptions.CheckCertificateRevocation = false;
    });
});


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
