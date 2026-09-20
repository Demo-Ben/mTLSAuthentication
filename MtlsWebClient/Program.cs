using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using YourBlazorNamespace.Components; // Replace with your actual project namespace

var builder = WebApplication.CreateBuilder(args);

// 1. Load the Client Certificate from the OS Certificate Store
X509Certificate2 clientCert;
using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
{
    store.Open(OpenFlags.ReadOnly);
    // Find the certificate by its Subject Name (adjust search criteria if needed, e.g., Thumbprint)
    var certificates = store.Certificates.Find(X509FindType.FindBySubjectName, "BlazorClientUser", false);

    if (certificates.Count == 0)
    {
        throw new InvalidOperationException("Client certificate 'BlazorClientUser' not found in the current user's store.");
    }

    clientCert = certificates[0];
}

// 2. Configure the HttpClient Handler with the Client Certificate
var handler = new HttpClientHandler();
handler.ClientCertificates.Add(clientCert);

// Optional: If you are hitting a development server using a self-signed LocalCA, 
// you can bypass chain errors if the CA isn't fully trusted by the application runtime store yet:
/*
handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
{
    // Custom validation logic if required for the server certificate
    return errors == System.Net.Security.SslPolicyErrors.None;
};
*/

// 3. Register the Named HttpClient
builder.Services.AddHttpClient("SecureApi", client =>
{
    // Set your target API base address here
    client.BaseAddress = new Uri("https://localhost:7001/");
})
.ConfigurePrimaryHttpMessageHandler(() => handler);

// 4. Add services to the container (Blazor Web App / Blazor Server setup)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerComponents();

app.Run();


//using MtlsWebClient.Components;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error", createScopeForErrors: true);
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();

//app.UseStaticFiles();
//app.UseAntiforgery();

//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode();

//app.Run();
