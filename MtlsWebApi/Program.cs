using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        // Require client certificates for mTLS
        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;

        // Custom validation logic to ensure the certificate is signed by your LocalCA
        httpsOptions.ClientCertificateValidation = (certificate, chain, errors) =>
        {
            // Add custom validation checks here if necessary
            // For local dev, checking if chain builds or matching thumbprint works
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapRazorComponents<App>()
   .AddInteractiveServerComponents();

app.Run();









//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
